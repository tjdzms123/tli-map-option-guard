using System.Drawing.Imaging;

namespace MapOptionGuard;

internal static class ScreenCapture
{
    /// <summary>화면의 지정 영역을 32비트 비트맵으로 복사한다.</summary>
    public static Bitmap Capture(Rectangle screenRect)
    {
        var bitmap = new Bitmap(screenRect.Width, screenRect.Height, PixelFormat.Format32bppArgb);
        try
        {
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(screenRect.Location, Point.Empty, screenRect.Size, CopyPixelOperation.SourceCopy);
            return bitmap;
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }
}
