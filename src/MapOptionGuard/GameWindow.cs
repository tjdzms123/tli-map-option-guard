using System.Runtime.InteropServices;

namespace MapOptionGuard;

/// <summary>
/// 창 제목으로 찾은 게임 창. 클라이언트 영역의 화면 좌표 계산과 포커스 확인을 담당한다.
/// </summary>
internal sealed class GameWindow
{
    private readonly IntPtr _handle;

    private GameWindow(IntPtr handle) => _handle = handle;

    public static GameWindow? Find(string title)
    {
        var handle = NativeMethods.FindWindow(null, title);
        return handle == IntPtr.Zero ? null : new GameWindow(handle);
    }

    public bool IsForeground => NativeMethods.GetForegroundWindow() == _handle;

    public bool IsMinimized => NativeMethods.IsIconic(_handle);

    /// <summary>제목 표시줄·테두리를 뺀 영역의 화면 좌표. 실패하면 빈 사각형.</summary>
    public Rectangle GetClientScreenRect()
    {
        if (!NativeMethods.GetClientRect(_handle, out var rect))
        {
            return Rectangle.Empty;
        }

        var origin = new NativeMethods.POINT();
        if (!NativeMethods.ClientToScreen(_handle, ref origin))
        {
            return Rectangle.Empty;
        }

        return new Rectangle(origin.X, origin.Y, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public void BringToFront() => NativeMethods.SetForegroundWindow(_handle);
}

internal static class NativeMethods
{
    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint WDA_EXCLUDEFROMCAPTURE = 0x0011;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(string? className, string windowName);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint affinity);
}
