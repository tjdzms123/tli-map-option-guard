using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace MapOptionGuard;

/// <summary>
/// 옵션 영역을 캡처하고, OCR이 읽기 쉽게 전처리한 뒤 Windows 내장 OCR로 글자를 읽는다.
/// </summary>
internal sealed class OptionScanner : IDisposable
{
    private readonly OcrEngine _engine;
    private Bitmap? _lastImage;

    private OptionScanner(OcrEngine engine) => _engine = engine;

    /// <summary>해당 언어 OCR이 설치되어 있지 않으면 null.</summary>
    public static OptionScanner? TryCreate(string languageTag)
    {
        var engine = OcrEngine.TryCreateFromLanguage(new Language(languageTag));
        return engine is null ? null : new OptionScanner(engine);
    }

    /// <summary>화면 영역을 읽어 줄 단위로 합친 글자를 돌려준다.</summary>
    public async Task<string> ReadTextAsync(Rectangle screenRect, double scale)
    {
        byte[] pixels;
        int width;
        int height;
        using (var captured = ScreenCapture.Capture(screenRect))
        {
            var prepared = PrepareForOcr(captured, scale, out pixels);
            width = prepared.Width;
            height = prepared.Height;
            _lastImage?.Dispose();
            _lastImage = prepared;
        }

        using var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(
            pixels.AsBuffer(), BitmapPixelFormat.Bgra8, width, height, BitmapAlphaMode.Premultiplied);
        var result = await _engine.RecognizeAsync(softwareBitmap);
        return string.Join("\n", result.Lines.Select(line => line.Text));
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

    // 확대한 뒤 RGB 중 가장 밝은 채널을 반전해 흑백으로 만든다.
    // 흰 글자뿐 아니라 빨간·주황 옵션 글자도 흰 배경 위 진한 글씨가 되어 인식률이 올라간다.
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
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    byte brightest = Math.Max(pixels[i], Math.Max(pixels[i + 1], pixels[i + 2]));
                    byte inverted = (byte)(255 - brightest);
                    pixels[i] = inverted;
                    pixels[i + 1] = inverted;
                    pixels[i + 2] = inverted;
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
}
