using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace MapOptionGuard;

/// <summary>
/// 옵션 영역을 캡처하고, OCR이 읽기 쉽게 전처리한 뒤 Windows 내장 OCR로 글자를 읽는다.
/// </summary>
internal sealed class OptionScanner : IDisposable
{
    private const byte Paper = 255;
    // 원본 기준 이 길이(px) 이상 이어지는 가로 검정 줄은 글자 획이 아니라 밑줄로 본다(글자 한 칸은 약 14px)
    private const int UnderlineMinLength = 24;
    private static readonly Regex PercentMisread = new(@"(?<=\d)0/0", RegexOptions.Compiled);

    private readonly OcrEngine _engine;
    private Bitmap? _lastImage;

    private OptionScanner(OcrEngine engine) => _engine = engine;

    /// <summary>해당 언어 OCR이 설치되어 있지 않으면 null.</summary>
    public static OptionScanner? TryCreate(string languageTag)
    {
        var engine = OcrEngine.TryCreateFromLanguage(new Language(languageTag));
        return engine is null ? null : new OptionScanner(engine);
    }

    /// <summary>화면 영역을 읽어 위에서부터 줄 단위 글자 목록을 돌려준다.</summary>
    public async Task<IReadOnlyList<string>> ReadLinesAsync(Rectangle screenRect, double scale)
    {
        using var captured = ScreenCapture.Capture(screenRect);
        return await ReadLinesAsync(captured, scale);
    }

    /// <summary>이미 가진 이미지(진단 모드의 저장 캡처 등)를 읽는다.</summary>
    public async Task<IReadOnlyList<string>> ReadLinesAsync(Bitmap image, double scale)
    {
        var prepared = PrepareForOcr(image, scale, out var pixels);
        int width = prepared.Width;
        int height = prepared.Height;
        _lastImage?.Dispose();
        _lastImage = prepared;

        using var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
            pixels.AsBuffer(), BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
        var result = await _engine.RecognizeAsync(softwareBitmap);
        return result.Lines.Select(line => FixCommonMisreads(line.Text)).ToList();
    }

    /// <summary>마지막으로 OCR에 넣은 이미지를 PNG로 저장한다(키워드 조정 확인용).</summary>
    public bool SaveLastImage(string path)
    {
        if (_lastImage is null)
        {
            return false;
        }

        _lastImage.Save(path, ImageFormat.Png);
        return true;
    }

    public void Dispose() => _lastImage?.Dispose();

    // 한국어 OCR은 게임 글꼴의 %를 "0/0"으로 읽는다(+180% → +1800/0). 숫자 바로 뒤의 0/0만 %로 되돌린다.
    private static string FixCommonMisreads(string line) => PercentMisread.Replace(line, "%");

    // 확대 → RGB 중 가장 밝은 채널을 반전 → 밑줄 제거 순서로 만든다.
    // 흰 글자뿐 아니라 빨간·주황·하늘색 옵션 글자도 밝은 배경 위 진한 글씨가 되어 인식률이 올라간다.
    // 글자 가장자리의 회색 단계는 OCR에 도움이 되므로 이진화하지 않고 남긴다.
    private static Bitmap PrepareForOcr(Bitmap source, double scale, out byte[] pixels)
    {
        double maxScale = (double)OcrEngine.MaxImageDimension / Math.Max(source.Width, source.Height);
        scale = Math.Min(scale, maxScale);
        int width = Math.Max(1, (int)(source.Width * scale));
        int height = Math.Max(1, (int)(source.Height * scale));

        var prepared = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        try
        {
            using (var graphics = Graphics.FromImage(prepared))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(source, new Rectangle(0, 0, width, height));
            }

            var data = prepared.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);
            try
            {
                // 32비트 형식은 stride가 항상 width * 4라서 SoftwareBitmap 버퍼로 그대로 쓸 수 있다
                pixels = new byte[data.Stride * height];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

                var gray = new byte[width * height];
                for (int p = 0; p < gray.Length; p++)
                {
                    int i = p * 4;
                    gray[p] = (byte)(255 - Math.Max(pixels[i], Math.Max(pixels[i + 1], pixels[i + 2])));
                }

                RemoveUnderlines(gray, width, height, OtsuThreshold(gray), (int)(UnderlineMinLength * scale));

                for (int p = 0; p < gray.Length; p++)
                {
                    int i = p * 4;
                    pixels[i] = gray[p];
                    pixels[i + 1] = gray[p];
                    pixels[i + 2] = gray[p];
                    pixels[i + 3] = 255;
                }

                // 디버그용 저장 이미지도 OCR 입력과 같도록 되돌려 쓴다
                Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            }
            finally
            {
                prepared.UnlockBits(data);
            }

            return prepared;
        }
        catch
        {
            prepared.Dispose();
            throw;
        }
    }

    // 글자와 배경 밝기를 가장 잘 가르는 경계값을 고른다(오츠 방법). 배경 무늬가 달라도 따로 맞출 필요가 없다.
    private static byte OtsuThreshold(byte[] gray)
    {
        var histogram = new long[256];
        foreach (var value in gray)
        {
            histogram[value]++;
        }

        double totalSum = 0;
        for (int level = 0; level < 256; level++)
        {
            totalSum += level * (double)histogram[level];
        }

        long darkCount = 0;
        double darkSum = 0;
        double bestVariance = -1;
        int bestLevel = 127;
        for (int level = 0; level < 256; level++)
        {
            darkCount += histogram[level];
            if (darkCount == 0)
            {
                continue;
            }

            long brightCount = gray.Length - darkCount;
            if (brightCount == 0)
            {
                break;
            }

            darkSum += level * (double)histogram[level];
            double meanGap = darkSum / darkCount - (totalSum - darkSum) / brightCount;
            double variance = (double)darkCount * brightCount * meanGap * meanGap;
            if (variance > bestVariance)
            {
                bestVariance = variance;
                bestLevel = level;
            }
        }

        return (byte)bestLevel;
    }

    // 밑줄 친 단어(약화, 마비 등)를 OCR이 통째로 놓치므로, 글자 획보다 훨씬 긴 가로 진한 줄을 배경색으로 지운다.
    // inkThreshold 이하(진한 쪽)를 글자·밑줄로 본다.
    private static void RemoveUnderlines(byte[] gray, int width, int height, byte inkThreshold, int minRunLength)
    {
        for (int y = 0; y < height; y++)
        {
            int rowStart = y * width;
            int runStart = -1;
            for (int x = 0; x <= width; x++)
            {
                bool isInk = x < width && gray[rowStart + x] <= inkThreshold;
                if (isInk && runStart < 0)
                {
                    runStart = x;
                }
                else if (!isInk && runStart >= 0)
                {
                    if (x - runStart >= minRunLength)
                    {
                        Array.Fill(gray, Paper, rowStart + runStart, x - runStart);
                    }

                    runStart = -1;
                }
            }
        }
    }
}
