using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MapOptionGuard;

/// <summary>
/// 게임 창. 클라이언트 영역의 화면 좌표 계산과 포커스 확인을 담당한다.
/// </summary>
internal sealed class GameWindow
{
    private readonly IntPtr _handle;

    private GameWindow(IntPtr handle, string processName)
    {
        _handle = handle;
        ProcessName = processName;
    }

    public string ProcessName { get; }

    public bool IsForeground => NativeMethods.GetForegroundWindow() == _handle;

    public bool IsMinimized => NativeMethods.IsIconic(_handle);

    /// <summary>
    /// 제목이 같은 보이는 창 중에서 프로세스 이름이 맞고 화면이 가장 큰 창을 게임으로 고른다.
    /// 런처나 TTD 같은 도구 창이 게임과 같은 제목을 써도 프로세스·크기로 걸러진다. 못 찾으면 이유를 함께 돌려준다.
    /// </summary>
    /// <param name="processName">비우면 프로세스 이름은 확인하지 않는다(.exe는 빼고 적는다)</param>
    public static GameWindow? Find(string title, string processName, out string failureMessage)
    {
        var candidates = FindCandidates(title);
        var game = candidates
            .Where(window => processName.Length == 0
                             || window.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
            .MaxBy(window => window.GetClientArea());

        failureMessage = game is not null ? ""
            : candidates.Count == 0 ? $"'{title}' 창을 찾지 못했습니다."
            : $"'{title}' 창은 있지만 프로세스가 '{processName}'이 아닙니다: "
              + string.Join(", ", candidates.Select(window => window.ProcessName).Distinct());
        return game;
    }

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

    private long GetClientArea()
    {
        var rect = GetClientScreenRect();
        return (long)rect.Width * rect.Height;
    }

    /// <summary>
    /// 제목이 같은 보이는 최상위 창을 모두 찾는다.
    /// 실제 게임 창 제목은 끝에 공백이 붙어 있어("Torchlight: Infinite  ") 앞뒤 공백은 빼고 비교한다.
    /// </summary>
    public static List<GameWindow> FindCandidates(string title)
    {
        var wantedTitle = title.Trim();
        var found = new List<GameWindow>();
        NativeMethods.EnumWindows((handle, _) =>
        {
            if (NativeMethods.IsWindowVisible(handle)
                && string.Equals(GetWindowTitle(handle).Trim(), wantedTitle, StringComparison.Ordinal))
            {
                NativeMethods.GetWindowThreadProcessId(handle, out uint processId);
                found.Add(new GameWindow(handle, GetProcessName(processId)));
            }

            return true;
        }, IntPtr.Zero);
        return found;
    }

    private static string GetWindowTitle(IntPtr handle)
    {
        int length = NativeMethods.GetWindowTextLength(handle);
        if (length == 0)
        {
            return "";
        }

        var buffer = new StringBuilder(length + 1);
        NativeMethods.GetWindowText(handle, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static string GetProcessName(uint processId)
    {
        try
        {
            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch (Exception)
        {
            // 그 사이 종료됐거나 권한이 없으면 이름 없이 둔다
            return "";
        }
    }
}

internal static class NativeMethods
{
    public static readonly IntPtr HWND_TOPMOST = new(-1);
    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint WDA_EXCLUDEFROMCAPTURE = 0x0011;

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

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
