using System.Diagnostics;

namespace MapOptionGuard;

/// <summary>
/// 트레이 아이콘과 주기 검사 타이머를 가진 앱 본체.
/// 게임 창이 앞에 있을 때만 옵션 영역을 읽고, 금지 키워드가 보이면 초기화 버튼에 네모를 띄운다.
/// </summary>
internal sealed class GuardContext : ApplicationContext
{
    // 한두 번 OCR이 흔들려도 네모가 깜빡이지 않도록, 연속으로 못 찾았을 때만 숨긴다
    private const int HideAfterMisses = 2;
    // 영역 설정 전에 게임 창이 앞으로 올라와 다시 그려질 때까지 기다리는 시간
    private const int FocusSettleDelayMs = 400;

    private readonly OptionScanner _scanner;
    private readonly OverlayForm _overlay;
    private readonly NotifyIcon _tray;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly System.Windows.Forms.Timer _timer;
    private AppConfig _config;
    private bool _scanning;
    private bool _calibrating;
    private int _missCount;
    private string _lastOcrText = "";
    private string _lastStatus = "아직 검사하지 않았습니다.";

    public GuardContext(AppConfig config, OptionScanner scanner)
    {
        _config = config;
        _scanner = scanner;
        _overlay = new OverlayForm(config.GetBoxColor(), config.BoxThickness);

        _pauseItem = new ToolStripMenuItem("일시정지") { CheckOnClick = true };
        _pauseItem.CheckedChanged += (_, _) => OnPauseChanged();

        var menu = new ContextMenuStrip();
        menu.Items.Add("영역 설정...", null, async (_, _) => await CalibrateAsync());
        menu.Items.Add("마지막 OCR 결과 보기", null, (_, _) => ShowLastResult());
        menu.Items.Add("설정 파일 열기", null, (_, _) => OpenConfigFile());
        menu.Items.Add("설정 다시 불러오기", null, (_, _) => ReloadConfig());
        menu.Items.Add(_pauseItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("종료", null, (_, _) => ExitThread());

        _tray = new NotifyIcon
        {
            Icon = SystemIcons.Shield,
            Text = Program.AppTitle,
            ContextMenuStrip = menu,
            Visible = true,
        };
        _tray.DoubleClick += (_, _) => ShowLastResult();
        _tray.ShowBalloonTip(3000, Program.AppTitle,
            "맵 기본 옵션 감시를 시작했습니다. 트레이 아이콘을 우클릭하면 설정할 수 있습니다.", ToolTipIcon.Info);

        _timer = new System.Windows.Forms.Timer { Interval = config.IntervalMs };
        _timer.Tick += async (_, _) => await ScanOnceAsync();
        _timer.Start();
    }

    private bool IsSuspended => _calibrating || _pauseItem.Checked;

    protected override void ExitThreadCore()
    {
        _timer.Stop();
        _timer.Dispose();
        _tray.Visible = false;
        _tray.Dispose();
        _overlay.Dispose();
        _scanner.Dispose();
        base.ExitThreadCore();
    }

    private async Task ScanOnceAsync()
    {
        if (_scanning || IsSuspended)
        {
            return;
        }

        _scanning = true;
        try
        {
            var game = GameWindow.Find(_config.WindowTitle);
            if (game is null || game.IsMinimized || !game.IsForeground)
            {
                _lastStatus = game is null
                    ? $"게임 창을 찾지 못했습니다. (창 제목: {_config.WindowTitle})"
                    : "게임 창이 앞에 있지 않아 검사를 쉬는 중입니다.";
                HideOverlay();
                return;
            }

            var client = game.GetClientScreenRect();
            var optionRect = _config.OptionRegion.ToScreen(client);
            if (optionRect.Width <= 0 || optionRect.Height <= 0)
            {
                _lastStatus = "옵션 영역 크기가 0입니다. '영역 설정'을 다시 해 주세요.";
                HideOverlay();
                return;
            }

            _lastOcrText = await _scanner.ReadTextAsync(optionRect, _config.OcrScale);
            if (IsSuspended)
            {
                // OCR을 기다리는 사이 일시정지나 영역 설정이 시작된 경우
                HideOverlay();
                return;
            }

            var compactText = KeywordMatcher.RemoveWhitespace(_lastOcrText);
            bool panelVisible = string.IsNullOrWhiteSpace(_config.PanelAnchor)
                                || KeywordMatcher.Contains(compactText, _config.PanelAnchor, _config.FuzzyMatch);
            var blocked = panelVisible
                ? KeywordMatcher.FindBlocked(compactText, _config.BlockedKeywords, _config.FuzzyMatch)
                : new List<string>();

            _lastStatus = !panelVisible
                ? $"'{_config.PanelAnchor}' 제목이 보이지 않아 옵션 창이 닫힌 것으로 봤습니다."
                : blocked.Count > 0 ? "걸린 키워드: " + string.Join(", ", blocked) : "걸린 키워드 없음";

            if (blocked.Count > 0)
            {
                _missCount = 0;
                _overlay.ShowAround(_config.ResetButtonRegion.ToScreen(client), blocked);
            }
            else if (++_missCount >= HideAfterMisses)
            {
                _overlay.HideBox();
            }
        }
        catch (Exception ex)
        {
            _lastStatus = "검사 중 오류: " + ex.Message;
            HideOverlay();
        }
        finally
        {
            _scanning = false;
        }
    }

    private async Task CalibrateAsync()
    {
        if (_calibrating)
        {
            return;
        }

        _calibrating = true;
        HideOverlay();
        try
        {
            var game = GameWindow.Find(_config.WindowTitle);
            if (game is null)
            {
                ShowMessage($"게임 창을 찾지 못했습니다.\n창 제목: {_config.WindowTitle}", MessageBoxIcon.Warning);
                return;
            }

            game.BringToFront();
            await Task.Delay(FocusSettleDelayMs);

            var client = game.GetClientScreenRect();
            if (game.IsMinimized || client.Width <= 0 || client.Height <= 0)
            {
                ShowMessage("게임 창이 최소화되어 있습니다. 맵 화면을 띄운 뒤 다시 시도하세요.", MessageBoxIcon.Warning);
                return;
            }

            using var screenshot = ScreenCapture.Capture(client);
            using var form = new CalibrationForm(screenshot, client);
            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _config.OptionRegion = RelativeRect.FromClient(form.OptionRect, client.Size);
            _config.ResetButtonRegion = RelativeRect.FromClient(form.ResetButtonRect, client.Size);
            _config.Save();
            _tray.ShowBalloonTip(2000, Program.AppTitle, "영역을 저장했습니다.", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            ShowMessage("영역 설정 중 오류가 났습니다.\n" + ex.Message, MessageBoxIcon.Error);
        }
        finally
        {
            _calibrating = false;
        }
    }

    private void ShowLastResult()
    {
        var imagePath = Path.Combine(AppContext.BaseDirectory, "last_ocr.png");
        string imageInfo;
        try
        {
            imageInfo = _scanner.SaveLastImage(imagePath)
                ? $"OCR에 넣은 이미지: {imagePath}"
                : "아직 캡처한 이미지가 없습니다.";
        }
        catch (Exception ex)
        {
            imageInfo = "이미지 저장 실패: " + ex.Message;
        }

        var text = string.IsNullOrWhiteSpace(_lastOcrText) ? "(읽은 글자 없음)" : _lastOcrText;
        ShowMessage($"상태: {_lastStatus}\n\n[읽은 글자]\n{text}\n\n{imageInfo}", MessageBoxIcon.Information);
    }

    private void OpenConfigFile()
    {
        try
        {
            Process.Start("notepad.exe", $"\"{AppConfig.FilePath}\"");
        }
        catch (Exception ex)
        {
            ShowMessage("설정 파일을 열지 못했습니다.\n" + ex.Message, MessageBoxIcon.Warning);
        }
    }

    private void ReloadConfig()
    {
        try
        {
            _config = AppConfig.LoadOrCreate();
            _overlay.SetStyle(_config.GetBoxColor(), _config.BoxThickness);
            _timer.Interval = _config.IntervalMs;
            HideOverlay();
            _tray.ShowBalloonTip(2000, Program.AppTitle, "설정을 다시 불러왔습니다.", ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            ShowMessage("설정 파일을 읽지 못해 이전 설정을 계속 씁니다.\n" + ex.Message, MessageBoxIcon.Warning);
        }
    }

    private void OnPauseChanged()
    {
        if (_pauseItem.Checked)
        {
            HideOverlay();
        }

        _tray.Text = _pauseItem.Checked ? $"{Program.AppTitle} (일시정지)" : Program.AppTitle;
    }

    private void HideOverlay()
    {
        _missCount = HideAfterMisses;
        _overlay.HideBox();
    }

    private static void ShowMessage(string text, MessageBoxIcon icon) =>
        MessageBox.Show(text, Program.AppTitle, MessageBoxButtons.OK, icon);
}
