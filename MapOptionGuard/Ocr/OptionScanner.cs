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

    // 아래 값은 모두 원본 캡처에서 줄 높이(글자 높이)를 1로 본 비율이다.
    // 글머리표를 찾을 줄 왼쪽 띠의 너비와, 띠와 첫 글자 사이 간격
    private const double BulletSearchWidth = 2.5;
    private const double BulletGap = 0.15;
    // 띠 안의 빨간 점이 (줄 높이)² × 이 비율 이상이면 글머리표로 본다
    private const double BulletMinArea = 0.05;
    // 줄 아래 빈 공간이 줄 높이 × 이 배수 이상이면, 다음 줄이 있었다면 보였을 것이므로 잘리지 않았다고 본다
    private const double SpaceForNextLine = 1.2;

    // OCR이 글머리표를 따로 한 낱말로 읽은 경우
    private static readonly string[] BulletWords = ["◆", "◇", "♦", "•", "·", "●", "■", "▪", "*", "❖"];

    private readonly OcrEngine _engine;
    private Bitmap? _lastImage;

    private OptionScanner(OcrEngine engine) => _engine = engine;

    /// <summary>해당 언어 OCR이 설치되어 있지 않으면 null.</summary>
    public static OptionScanner? TryCreate(string languageTag)
    {
        var engine = OcrEngine.TryCreateFromLanguage(new Language(languageTag));
        return engine is null ? null : new OptionScanner(engine);
    }

    /// <summary>화면 영역을 읽어 위에서부터 줄 목록을 돌려준다.</summary>
    public async Task<IReadOnlyList<OcrTextLine>> ReadLinesAsync(Rectangle screenRect, double scale)
    {
        using var captured = ScreenCapture.Capture(screenRect);
        return await ReadLinesAsync(captured, scale);
    }

    /// <summary>
    /// 이미 가진 이미지(진단 모드의 저장 캡처 등)를 읽는다.
    /// 글자는 전처리한 이미지로 읽고, 글머리표(빨간 ◆)는 색이 남아 있는 원본에서 찾는다.
    /// </summary>
    public async Task<IReadOnlyList<OcrTextLine>> ReadLinesAsync(Bitmap image, double scale)
    {
        var prepared = PrepareForOcr(image, scale, out var pixels, out double usedScale);
        int width = prepared.Width;
        int height = prepared.Height;
        _lastImage?.Dispose();
        _lastImage = prepared;

        using var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
            pixels.AsBuffer(), BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
        var result = await _engine.RecognizeAsync(softwareBitmap);
        var redMask = RedMask.From(image);
        return result.Lines.Select(line => ToTextLine(line, usedScale, redMask)).ToList();
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

    private static OcrTextLine ToTextLine(OcrLine line, double scale, RedMask redMask)
    {
        var words = line.Words.ToList();
        bool bulletWord = words.Count > 1 && BulletWords.Contains(words[0].Text);
        if (bulletWord)
        {
            words.RemoveAt(0);
        }

        var text = FixCommonMisreads(bulletWord ? string.Join(" ", words.Select(word => word.Text)) : line.Text);
        var bounds = ToSourceRect(words, scale);
        if (bounds.Height <= 0)
        {
            return new OcrTextLine(text, bulletWord);
        }

        bool hasSpaceBelow = redMask.Height - bounds.Bottom >= bounds.Height * SpaceForNextLine;
        return new OcrTextLine(text, bulletWord || HasBulletLeftOf(bounds, redMask), hasSpaceBelow);
    }

    // OCR 낱말 위치는 확대한 이미지 기준이라 원본 픽셀로 되돌린다
    private static Rectangle ToSourceRect(List<OcrWord> words, double scale)
    {
        if (words.Count == 0)
        {
            return Rectangle.Empty;
        }

        double left = words.Min(word => word.BoundingRect.X);
        double top = words.Min(word => word.BoundingRect.Y);
        double right = words.Max(word => word.BoundingRect.X + word.BoundingRect.Width);
        double bottom = words.Max(word => word.BoundingRect.Y + word.BoundingRect.Height);
        return Rectangle.FromLTRB((int)(left / scale), (int)(top / scale),
            (int)Math.Ceiling(right / scale), (int)Math.Ceiling(bottom / scale));
    }

    // 글머리표는 줄 첫 글자 왼쪽에 있다. 그 왼쪽 띠에 빨간 점이 충분히 있으면 옵션 첫 줄로 본다.
    private static bool HasBulletLeftOf(Rectangle line, RedMask redMask)
    {
        int lineHeight = line.Height;
        var band = Rectangle.FromLTRB(
            line.Left - (int)(lineHeight * BulletSearchWidth), line.Top,
            line.Left - (int)Math.Ceiling(lineHeight * BulletGap), line.Bottom);
        return redMask.CountIn(band) >= Math.Max(3, lineHeight * lineHeight * BulletMinArea);
    }

    // 확대 → RGB 중 가장 밝은 채널을 반전 → 밑줄 제거 순서로 만든다.
    // 흰 글자뿐 아니라 빨간·주황·하늘색 옵션 글자도 밝은 배경 위 진한 글씨가 되어 인식률이 올라간다.
    // 글자 가장자리의 회색 단계는 OCR에 도움이 되므로 이진화하지 않고 남긴다.
    private static Bitmap PrepareForOcr(Bitmap source, double scale, out byte[] pixels, out double usedScale)
    {
        double maxScale = (double)OcrEngine.MaxImageDimension / Math.Max(source.Width, source.Height);
        scale = Math.Min(scale, maxScale);
        usedScale = scale;
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

    /// <summary>원본 캡처에서 글머리표(빨간 ◆) 색인 픽셀 위치.</summary>
    private sealed class RedMask(bool[] isRed, int width, int height)
    {
        public int Height => height;

        public static RedMask From(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            var data = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                var bytes = new byte[data.Stride * height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                var isRed = new bool[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // 32비트 형식의 바이트 순서는 B, G, R, A
                        int i = y * data.Stride + x * 4;
                        isRed[y * width + x] = IsBulletRed(bytes[i + 2], bytes[i + 1], bytes[i]);
                    }
                }

                return new RedMask(isRed, width, height);
            }
            finally
            {
                image.UnlockBits(data);
            }
        }

        public int CountIn(Rectangle area)
        {
            int left = Math.Max(0, area.Left);
            int right = Math.Min(width, area.Right);
            int top = Math.Max(0, area.Top);
            int bottom = Math.Min(height, area.Bottom);
            int count = 0;
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    if (isRed[y * width + x])
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // 빨강이 뚜렷하고 초록·파랑보다 1.5배 이상 센 색. 어두운 배경 무늬는 빨강이 약해서 걸리지 않는다.
        private static bool IsBulletRed(int r, int g, int b) => r >= 110 && r * 2 >= g * 3 && r * 2 >= b * 3;
    }
}
