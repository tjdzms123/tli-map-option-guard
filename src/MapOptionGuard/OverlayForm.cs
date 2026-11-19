using System.Drawing.Drawing2D;

namespace MapOptionGuard;

/// <summary>
/// 초기화 버튼 위에 네모와 사유를 그리는 투명 창. 클릭은 아래 게임으로 그대로 통과한다.
/// </summary>
internal sealed class OverlayForm : Form
{
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int BoxPadding = 6;
    private static readonly Color KeyColor = Color.Magenta;

    private Color _boxColor;
    private int _thickness;
    private Rectangle _boxRect;
    private Rectangle _labelRect;
    private string _label = "";

    public OverlayForm(Color boxColor, int thickness)
    {
        _boxColor = boxColor;
        _thickness = thickness;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        AutoScaleMode = AutoScaleMode.None;
        TopMost = true;
        BackColor = KeyColor;
        TransparencyKey = KeyColor;
        DoubleBuffered = true;
        Font = new Font("맑은 고딕", 10f, FontStyle.Bold);
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var createParams = base.CreateParams;
            createParams.ExStyle |= WS_EX_TOPMOST | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_LAYERED | WS_EX_NOACTIVATE;
            return createParams;
        }
    }

    public void SetStyle(Color boxColor, int thickness)
    {
        _boxColor = boxColor;
        _thickness = thickness;
        Invalidate();
    }

    /// <summary>버튼 영역(화면 좌표)을 감싸는 네모와, 그 아래에 걸린 키워드를 표시한다.</summary>
    public void ShowAround(Rectangle buttonRect, IReadOnlyList<string> reasons)
    {
        var label = "초기화 권장: " + string.Join(", ", reasons);
        var textSize = TextRenderer.MeasureText(label, Font);
        var boxRect = new Rectangle(0, 0, buttonRect.Width + BoxPadding * 2, buttonRect.Height + BoxPadding * 2);
        var labelRect = new Rectangle(0, boxRect.Bottom, textSize.Width + 12, textSize.Height + 6);
        var bounds = new Rectangle(buttonRect.X - BoxPadding, buttonRect.Y - BoxPadding,
            Math.Max(boxRect.Width, labelRect.Width), boxRect.Height + labelRect.Height);

        bool changed = label != _label || boxRect != _boxRect;
        _label = label;
        _boxRect = boxRect;
        _labelRect = labelRect;

        if (Bounds != bounds)
        {
            Bounds = bounds;
        }

        if (!Visible)
        {
            Show();
        }

        // 게임이 자기 창을 최상위로 올려도 네모가 가려지지 않도록 매번 최상위로 다시 올린다
        NativeMethods.SetWindowPos(Handle, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);

        if (changed)
        {
            Invalidate();
        }
    }

    public void HideBox()
    {
        if (Visible)
        {
            Hide();
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // 이 창이 옵션 캡처에 찍혀 라벨 글자가 다시 키워드로 읽히는 것을 막는다(녹화·스크린샷에도 안 보인다)
        NativeMethods.SetWindowDisplayAffinity(Handle, NativeMethods.WDA_EXCLUDEFROMCAPTURE);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // 투명 키 색과 섞여 테두리가 번지지 않도록 안티앨리어싱 없이 그린다
        e.Graphics.SmoothingMode = SmoothingMode.None;
        using (var pen = new Pen(_boxColor, _thickness) { Alignment = PenAlignment.Inset })
        {
            e.Graphics.DrawRectangle(pen, _boxRect);
        }

        using (var brush = new SolidBrush(_boxColor))
        {
            e.Graphics.FillRectangle(brush, _labelRect);
        }

        TextRenderer.DrawText(e.Graphics, _label, Font, _labelRect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
    }
}
