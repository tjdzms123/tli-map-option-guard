using System.Diagnostics;

namespace MapOptionGuard;

/// <summary>
/// 앱 메인 창. 주기적으로 검사를 돌려 상태·인식 결과를 보여 주고, 걸린 옵션이 있으면
/// 초기화 버튼 위에 오버레이 네모를 띄운다. 이 창을 닫으면 프로그램이 끝난다.
/// 컨트롤 배치는 MainForm.Designer.cs(VS 디자이너)에서, 동작은 이 파일에서 다룬다.
/// </summary>
internal sealed partial class MainForm : Form
{
    // 한두 번 OCR이 흔들려도 네모가 깜빡이지 않도록, 연속으로 못 찾았을 때만 숨긴다
    private const int HideAfterMisses = 2;
    // 영역 설정 전에 게임 창이 앞으로 올라와 다시 그려질 때까지 기다리는 시간
    private const int FocusSettleDelayMs = 400;
    // 검사 간격 슬라이더 한 칸의 크기
    private const int IntervalStepMs = 100;

    private static readonly Color BlockedColor = Color.Firebrick;
    private static readonly Color CleanColor = Color.SeaGreen;
    private static readonly Color IdleColor = Color.DimGray;
    private static readonly Color WarningColor = Color.DarkOrange;

    private readonly OptionGuard _guard;
    private readonly OverlayForm _overlay;

    private AppConfig _config;
    private OptionPickerForm? _pickerForm;
    private bool _paused;
    private bool _scanning;
    private bool _calibrating;
    private bool _loadingUi;
    private int _missCount;

    public MainForm(AppConfig config, OptionScanner scanner, OptionCollector collector)
    {
        InitializeComponent();
        // 시스템 아이콘이라 디자이너가 아닌 코드에서 지정한다
        Icon = SystemIcons.Shield;

        _config = config;
        _guard = new OptionGuard(config, scanner, collector);
        _overlay = new OverlayForm(config.GetBoxColor(), config.BoxThickness);

        ApplyConfigToUi();
        ShowNotice(AppConfig.FilePath);
        scanTimer.Start();
    }

    private bool IsSuspended => _paused || _calibrating || IsDisposed;

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        scanTimer.Stop();
        _pickerForm?.Dispose();
        _overlay.Dispose();
        _guard.Dispose();
        base.OnFormClosed(e);
    }

    private async void ScanTimer_Tick(object? sender, EventArgs e)
    {
        if (_scanning || IsSuspended)
        {
            return;
        }

        _scanning = true;
        try
        {
            var result = await _guard.ScanAsync();
            if (IsSuspended)
            {
                // OCR을 기다리는 사이 중지·영역 설정·종료가 일어난 경우
                if (!IsDisposed)
                {
                    HideOverlay();
                }

                return;
            }

            ShowResult(result);
            UpdateOverlay(result);
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                SetStatus(scanStatusLabel, "검사 중 오류: " + ex.Message, WarningColor);
                HideOverlay();
            }
        }
        finally
        {
            _scanning = false;
        }
    }

    private void ApplyGameWindowButton_Click(object? sender, EventArgs e)
    {
        var title = windowTitleBox.Text.Trim();
        if (title.Length == 0)
        {
            ShowMessage("창 제목을 입력하세요.", MessageBoxIcon.Warning);
            return;
        }

        _config.WindowTitle = title;
        _config.ProcessName = AppConfig.NormalizeProcessName(processNameBox.Text);
        processNameBox.Text = _config.ProcessName;
        TrySaveConfig("게임 창 설정을 저장했습니다.");
    }

    private void ToggleButton_Click(object? sender, EventArgs e)
    {
        _paused = !_paused;
        toggleButton.Text = _paused ? "시작 ▶" : "중지 ■";
        if (_paused)
        {
            HideOverlay();
            SetStatus(scanStatusLabel, "중지됨", IdleColor);
        }
    }

    private async void CalibrateButton_Click(object? sender, EventArgs e)
    {
        if (_calibrating)
        {
            return;
        }

        _calibrating = true;
        HideOverlay();
        try
        {
            var game = GameWindow.Find(_config.WindowTitle, _config.ProcessName, out var failureMessage);
            if (game is null)
            {
                ShowMessage(failureMessage, MessageBoxIcon.Warning);
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
            if (form.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            _config.OptionRegion = RelativeRect.FromClient(form.OptionRect, client.Size);
            _config.ResetButtonRegion = RelativeRect.FromClient(form.ResetButtonRect, client.Size);
            TrySaveConfig("영역을 저장했습니다.");
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

    private void IntervalBar_ValueChanged(object? sender, EventArgs e)
    {
        if (_loadingUi)
        {
            return;
        }

        _config.IntervalMs = intervalBar.Value * IntervalStepMs;
        scanTimer.Interval = _config.IntervalMs;
        UpdateIntervalLabel();
        TrySaveConfig(null);
    }

    private void OpenOcrImageButton_Click(object? sender, EventArgs e)
    {
        var imagePath = Path.Combine(AppContext.BaseDirectory, "last_ocr.png");
        try
        {
            if (!_guard.SaveLastImage(imagePath))
            {
                ShowMessage("아직 캡처한 이미지가 없습니다. 게임 창을 맨 앞에 띄워 한 번 검사되게 하세요.",
                    MessageBoxIcon.Information);
                return;
            }

            Process.Start(new ProcessStartInfo(imagePath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowMessage("이미지를 열지 못했습니다.\n" + ex.Message, MessageBoxIcon.Warning);
        }
    }

    private void ApplyKeywordsButton_Click(object? sender, EventArgs e)
    {
        _config.BlockedKeywords = keywordsBox.Text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();
        keywordsBox.Text = string.Join(", ", _config.BlockedKeywords);
        TrySaveConfig($"직접 키워드 {_config.BlockedKeywords.Count}개를 저장했습니다.");
    }

    private void CollectCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (_loadingUi)
        {
            return;
        }

        _config.CollectOptions = collectCheckBox.Checked;
        TrySaveConfig(_config.CollectOptions ? "옵션 모으기를 켰습니다." : "옵션 모으기를 껐습니다.");
    }

    private void OpenPickerButton_Click(object? sender, EventArgs e)
    {
        if (_pickerForm is { IsDisposed: false })
        {
            _pickerForm.Activate();
            return;
        }

        _pickerForm = new OptionPickerForm(_guard.Collector);
        _pickerForm.FormClosed += (_, _) => UpdateCollectSummary();
        _pickerForm.Show(this);
    }

    private void OpenConfigButton_Click(object? sender, EventArgs e)
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

    private void ReloadConfigButton_Click(object? sender, EventArgs e)
    {
        try
        {
            _config = AppConfig.LoadOrCreate();
            _guard.Config = _config;
            _overlay.SetStyle(_config.GetBoxColor(), _config.BoxThickness);
            ApplyConfigToUi();
            HideOverlay();
            ShowNotice("설정을 다시 불러왔습니다.");
        }
        catch (Exception ex)
        {
            ShowMessage("설정 파일을 읽지 못해 이전 설정을 계속 씁니다.\n" + ex.Message, MessageBoxIcon.Warning);
        }
    }

    private void ShowResult(ScanResult result)
    {
        bool gameFound = result.State != ScanState.GameNotFound;
        SetStatus(gameStatusLabel,
            gameFound
                ? $"연결됨 · {result.GameProcessName} · 게임 화면 {result.GameClient.Width}×{result.GameClient.Height}"
                : "찾지 못함 · " + result.Message,
            gameFound ? CleanColor : WarningColor);
        SetStatus(scanStatusLabel, result.Message, StatusColor(result.State));

        if (result.RanOcr)
        {
            SetText(ocrTextBox, result.OcrText.Replace("\n", Environment.NewLine));
        }

        UpdateCollectSummary();
    }

    private void UpdateOverlay(ScanResult result)
    {
        switch (result.State)
        {
            case ScanState.Blocked:
                _missCount = 0;
                _overlay.ShowAround(result.ResetButtonRect, result.Blocked);
                break;
            case ScanState.Clean:
            case ScanState.PanelClosed:
                if (++_missCount >= HideAfterMisses)
                {
                    _overlay.HideBox();
                }

                break;
            default:
                HideOverlay();
                break;
        }
    }

    // 설정값을 화면에 옮긴다. 이때 생기는 변경 이벤트가 다시 저장하지 않도록 막는다.
    private void ApplyConfigToUi()
    {
        _loadingUi = true;
        try
        {
            windowTitleBox.Text = _config.WindowTitle;
            processNameBox.Text = _config.ProcessName;
            intervalBar.Value = Math.Clamp(_config.IntervalMs / IntervalStepMs, intervalBar.Minimum, intervalBar.Maximum);
            keywordsBox.Text = string.Join(", ", _config.BlockedKeywords);
            collectCheckBox.Checked = _config.CollectOptions;
        }
        finally
        {
            _loadingUi = false;
        }

        scanTimer.Interval = _config.IntervalMs;
        UpdateIntervalLabel();
        UpdateCollectSummary();
    }

    private void TrySaveConfig(string? doneMessage)
    {
        try
        {
            _config.Save();
            if (doneMessage is not null)
            {
                ShowNotice(doneMessage);
            }
        }
        catch (Exception ex)
        {
            ShowMessage("설정을 저장하지 못했습니다.\n" + ex.Message, MessageBoxIcon.Warning);
        }
    }

    private void UpdateIntervalLabel() => intervalLabel.Text = $"{_config.IntervalMs / 1000.0:0.0} s";

    private void UpdateCollectSummary()
    {
        var collector = _guard.Collector;
        SetText(collectSummaryLabel,
            $"모은 옵션 {collector.Count}개 · 거르는 옵션 {collector.Options.Count(o => o.Blocked)}개");
    }

    private void HideOverlay()
    {
        _missCount = HideAfterMisses;
        _overlay.HideBox();
    }

    private void ShowNotice(string message) => noticeLabel.Text = $"{DateTime.Now:HH:mm:ss}  {message}";

    private static Color StatusColor(ScanState state) => state switch
    {
        ScanState.Blocked => BlockedColor,
        ScanState.Clean => CleanColor,
        ScanState.GameNotFound or ScanState.InvalidRegion => WarningColor,
        _ => IdleColor,
    };

    // 0.5초마다 같은 글자를 다시 넣으면 깜빡이거나 선택이 풀리므로 바뀔 때만 넣는다
    private static void SetText(Control control, string text)
    {
        if (control.Text != text)
        {
            control.Text = text;
        }
    }

    private static void SetStatus(Label label, string text, Color color)
    {
        SetText(label, text);
        label.ForeColor = color;
    }

    private static void ShowMessage(string text, MessageBoxIcon icon) =>
        MessageBox.Show(text, Program.AppTitle, MessageBoxButtons.OK, icon);
}
