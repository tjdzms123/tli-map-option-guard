using System.Drawing.Drawing2D;

namespace MapOptionGuard;

/// <summary>
/// 게임 화면 캡처 위에서 옵션 영역 → 초기화 버튼 → 포탈 개방 버튼 순서로 드래그해 위치를 지정받는 창.
/// 창은 게임 클라이언트 영역과 같은 위치·크기로 뜨므로 창 안 좌표가 곧 클라이언트 좌표다.
/// 화면을 직접 그리는 창이라 VS에서 디자이너 대신 코드 보기로 열리게 한다.
/// </summary>
[System.ComponentModel.DesignerCategory("Code")]
internal sealed class CalibrationForm : Form
{
    private const int MinDragSize = 5;
    private const string KeyHelp = "우클릭: 이전 단계    Esc: 취소";

    private static readonly string[] StepGuides =
    [
        // 옵션 아래 빈 공간까지 잡아야 목록 끝이 보이는지(아래에 옵션이 더 없는지) 알 수 있다
        "1/3  '기본 옵션' 제목부터 옵션 창 맨 아래(빈 공간 포함)까지 드래그하세요",
        "2/3  '옵션 초기화' 버튼을 드래그하세요",
        "3/3  '포탈 개방' 버튼을 드래그하세요",
    ];

    private static readonly Color[] StepColors = [Color.Cyan, Color.Red, Color.Lime];

    private readonly Bitmap _screenshot;
    private readonly Rectangle[] _selections = new Rectangle[StepGuides.Length];
    private int _step;
    private Point _dragStart;
    private Rectangle? _dragRect;

    public CalibrationForm(Bitmap screenshot, Rectangle clientScreenRect)
    {
        _screenshot = screenshot;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        AutoScaleMode = AutoScaleMode.None;
        ShowInTaskbar = false;
        TopMost = true;
        DoubleBuffered = true;
        KeyPreview = true;
        Cursor = Cursors.Cross;
        Font = new Font("맑은 고딕", 14f, FontStyle.Bold);
        Bounds = clientScreenRect;
    }

    public Rectangle OptionRect => _selections[0];

    public Rectangle ResetButtonRect => _selections[1];

    public Rectangle PortalButtonRect => _selections[2];

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        Activate();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
        }

        base.OnKeyDown(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Right)
        {
            _dragRect = null;
            _step = Math.Max(0, _step - 1);
            Invalidate();
            return;
        }

        if (e.Button == MouseButtons.Left)
        {
            _dragStart = e.Location;
            _dragRect = new Rectangle(e.Location, Size.Empty);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragRect is null)
        {
            return;
        }

        _dragRect = RectFromPoints(_dragStart, e.Location);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button != MouseButtons.Left || _dragRect is not { } rect)
        {
            return;
        }

        _dragRect = null;
        if (rect.Width >= MinDragSize && rect.Height >= MinDragSize)
        {
            _selections[_step++] = rect;
            if (_step == _selections.Length)
            {
                DialogResult = DialogResult.OK;
                return;
            }
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        // 이미지 DPI 정보와 무관하게 픽셀 1:1로 그린다
        graphics.DrawImage(_screenshot, new Rectangle(Point.Empty, _screenshot.Size));

        for (int i = 0; i < _step; i++)
        {
            DrawSelection(graphics, _selections[i], StepColors[i]);
        }

        int currentStep = Math.Min(_step, StepGuides.Length - 1);
        if (_dragRect is { } drag)
        {
            DrawSelection(graphics, drag, StepColors[currentStep]);
        }

        DrawGuide(graphics, StepGuides[currentStep] + "\n" + KeyHelp);
    }

    private static void DrawSelection(Graphics graphics, Rectangle rect, Color color)
    {
        using var pen = new Pen(color, 3) { DashStyle = DashStyle.Dash };
        graphics.DrawRectangle(pen, rect);
    }

    private void DrawGuide(Graphics graphics, string text)
    {
        var textSize = TextRenderer.MeasureText(text, Font);
        var box = new Rectangle((ClientSize.Width - textSize.Width) / 2 - 16, 20, textSize.Width + 32, textSize.Height + 16);
        using (var brush = new SolidBrush(Color.FromArgb(210, 0, 0, 0)))
        {
            graphics.FillRectangle(brush, box);
        }

        var textRect = Rectangle.Inflate(box, -16, -8);
        TextRenderer.DrawText(graphics, text, Font, textRect, Color.White, TextFormatFlags.HorizontalCenter);
    }

    private static Rectangle RectFromPoints(Point a, Point b) => Rectangle.FromLTRB(
        Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
}
