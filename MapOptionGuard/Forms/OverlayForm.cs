using System.Drawing.Drawing2D;

namespace MapOptionGuard;

/// <summary>
/// 게임 버튼 위에 네모와 안내 글을 그리는 투명 창. 클릭은 아래 게임으로 그대로 통과한다.
/// 걸린 옵션이 있으면 초기화 버튼에 빨간 네모를, 거를 옵션이 없으면 포탈 개방 버튼에 초록 네모를 띄운다.
/// 화면을 직접 그리는 창이라 VS에서 디자이너 대신 코드 보기로 열리게 한다.
/// </summary>
[System.ComponentModel.DesignerCategory("Code")]
internal sealed class OverlayForm : Form
{
    private const int WS_EX_TOPMOST = 0x00000008;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int BoxPadding = 6;
    // 거르기로 고른 옵션은 문장이 길어서 라벨에는 앞부분만 보여 준다
    private const int MaxReasonLength = 16;
    private static readonly Color KeyColor = Color.Magenta;

    private Color _warningColor;
    private Color _okColor;
    private int _thickness;
    private Color _boxColor;
    private Rectangle _boxRect;
    private Rectangle _labelRect;
    private string _label = "";

    public OverlayForm(Color warningColor, Color okColor, int thickness)
    {
        _warningColor = warningColor;
        _okColor = okColor;
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

    public void SetStyle(Color warningColor, Color okColor, int thickness)
    {
        _warningColor = warningColor;
        _okColor = okColor;
        _thickness = thickness;
        Invalidate();
    }

    /// <summary>초기화 버튼(화면 좌표)을 빨간 네모로 감싸고 걸린 옵션을 표시한다.</summary>
    public void ShowResetWarning(Rectangle resetButtonRect, IReadOnlyList<string> reasons) =>
        ShowAround(resetButtonRect, "초기화 권장: " + string.Join(", ", reasons.Select(Shorten)), _warningColor);

    /// <summary>포탈 개방 버튼(화면 좌표)을 초록 네모로 감싸 거를 옵션이 없음을 표시한다.</summary>
    public void ShowPortalOk(Rectangle portalButtonRect) => ShowAround(portalButtonRect, "거를 옵션 없음", _okColor);

    public void HideBox()
    {
        if (Visible)
        {
            Hide();
        }
    }

    private static string Shorten(string reason) =>
        reason.Length <= MaxReasonLength ? reason : reason[..MaxReasonLength] + "…";

    // 버튼을 감싸는 네모와 안내 글을 띄운다. 글은 네모 아래에 두되, 화면 아래로 넘치면 네모 위에 둔다.
    private void ShowAround(Rectangle buttonRect, string label, Color color)
    {
        var textSize = TextRenderer.MeasureText(label, Font);
        var boxSize = new Size(buttonRect.Width + BoxPadding * 2, buttonRect.Height + BoxPadding * 2);
        var labelSize = new Size(textSize.Width + 12, textSize.Height + 6);
        int screenBottom = Screen.FromRectangle(buttonRect).Bounds.Bottom;
        bool labelAbove = buttonRect.Bottom + BoxPadding + labelSize.Height > screenBottom;

        var boxRect = new Rectangle(new Point(0, labelAbove ? labelSize.Height : 0), boxSize);
        var labelRect = new Rectangle(new Point(0, labelAbove ? 0 : boxRect.Bottom), labelSize);
        int top = buttonRect.Y - BoxPadding - (labelAbove ? labelSize.Height : 0);
        var bounds = new Rectangle(buttonRect.X - BoxPadding, top,
            Math.Max(boxSize.Width, labelSize.Width), boxSize.Height + labelSize.Height);

        bool changed = label != _label || boxRect != _boxRect || color != _boxColor;
        _label = label;
        _boxRect = boxRect;
        _labelRect = labelRect;
        _boxColor = color;

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
