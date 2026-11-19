#nullable disable

namespace MapOptionGuard
{
    partial class MainForm
    {
        /// <summary>디자이너 지원에 필요한 변수입니다.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>사용 중인 모든 리소스를 정리합니다.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.rootPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.gameHeaderLabel = new System.Windows.Forms.Label();
            this.gameRow = new System.Windows.Forms.FlowLayoutPanel();
            this.windowTitleLabel = new System.Windows.Forms.Label();
            this.windowTitleBox = new System.Windows.Forms.TextBox();
            this.processNameLabel = new System.Windows.Forms.Label();
            this.processNameBox = new System.Windows.Forms.TextBox();
            this.applyGameWindowButton = new System.Windows.Forms.Button();
            this.gameStatusLabel = new System.Windows.Forms.Label();
            this.watchHeaderLabel = new System.Windows.Forms.Label();
            this.watchRow = new System.Windows.Forms.FlowLayoutPanel();
            this.toggleButton = new System.Windows.Forms.Button();
            this.calibrateButton = new System.Windows.Forms.Button();
            this.intervalCaptionLabel = new System.Windows.Forms.Label();
            this.intervalBar = new System.Windows.Forms.TrackBar();
            this.intervalLabel = new System.Windows.Forms.Label();
            this.scanStatusLabel = new System.Windows.Forms.Label();
            this.ocrHeaderLabel = new System.Windows.Forms.Label();
            this.ocrTextBox = new System.Windows.Forms.TextBox();
            this.openOcrImageButton = new System.Windows.Forms.Button();
            this.filterHeaderLabel = new System.Windows.Forms.Label();
            this.keywordsCaptionLabel = new System.Windows.Forms.Label();
            this.keywordsRow = new System.Windows.Forms.FlowLayoutPanel();
            this.keywordsBox = new System.Windows.Forms.TextBox();
            this.applyKeywordsButton = new System.Windows.Forms.Button();
            this.collectRow = new System.Windows.Forms.FlowLayoutPanel();
            this.collectCheckBox = new System.Windows.Forms.CheckBox();
            this.openPickerButton = new System.Windows.Forms.Button();
            this.collectSummaryLabel = new System.Windows.Forms.Label();
            this.configHeaderLabel = new System.Windows.Forms.Label();
            this.configRow = new System.Windows.Forms.FlowLayoutPanel();
            this.openConfigButton = new System.Windows.Forms.Button();
            this.reloadConfigButton = new System.Windows.Forms.Button();
            this.noticeLabel = new System.Windows.Forms.Label();
            this.scanTimer = new System.Windows.Forms.Timer(this.components);
            this.rootPanel.SuspendLayout();
            this.gameRow.SuspendLayout();
            this.watchRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.intervalBar)).BeginInit();
            this.keywordsRow.SuspendLayout();
            this.collectRow.SuspendLayout();
            this.configRow.SuspendLayout();
            this.SuspendLayout();
            //
            // rootPanel
            //
            this.rootPanel.AutoSize = true;
            this.rootPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rootPanel.Controls.Add(this.gameHeaderLabel);
            this.rootPanel.Controls.Add(this.gameRow);
            this.rootPanel.Controls.Add(this.gameStatusLabel);
            this.rootPanel.Controls.Add(this.watchHeaderLabel);
            this.rootPanel.Controls.Add(this.watchRow);
            this.rootPanel.Controls.Add(this.scanStatusLabel);
            this.rootPanel.Controls.Add(this.ocrHeaderLabel);
            this.rootPanel.Controls.Add(this.ocrTextBox);
            this.rootPanel.Controls.Add(this.openOcrImageButton);
            this.rootPanel.Controls.Add(this.filterHeaderLabel);
            this.rootPanel.Controls.Add(this.keywordsCaptionLabel);
            this.rootPanel.Controls.Add(this.keywordsRow);
            this.rootPanel.Controls.Add(this.collectRow);
            this.rootPanel.Controls.Add(this.collectSummaryLabel);
            this.rootPanel.Controls.Add(this.configHeaderLabel);
            this.rootPanel.Controls.Add(this.configRow);
            this.rootPanel.Controls.Add(this.noticeLabel);
            this.rootPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.rootPanel.Location = new System.Drawing.Point(0, 0);
            this.rootPanel.Name = "rootPanel";
            this.rootPanel.Padding = new System.Windows.Forms.Padding(10);
            this.rootPanel.Size = new System.Drawing.Size(530, 640);
            this.rootPanel.TabIndex = 0;
            this.rootPanel.WrapContents = false;
            //
            // gameHeaderLabel
            //
            this.gameHeaderLabel.AutoSize = true;
            this.gameHeaderLabel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.gameHeaderLabel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.gameHeaderLabel.Name = "gameHeaderLabel";
            this.gameHeaderLabel.Size = new System.Drawing.Size(44, 15);
            this.gameHeaderLabel.TabIndex = 0;
            this.gameHeaderLabel.Text = "게임 창";
            //
            // gameRow
            //
            this.gameRow.AutoSize = true;
            this.gameRow.Controls.Add(this.windowTitleLabel);
            this.gameRow.Controls.Add(this.windowTitleBox);
            this.gameRow.Controls.Add(this.processNameLabel);
            this.gameRow.Controls.Add(this.processNameBox);
            this.gameRow.Controls.Add(this.applyGameWindowButton);
            this.gameRow.Margin = new System.Windows.Forms.Padding(0);
            this.gameRow.Name = "gameRow";
            this.gameRow.Size = new System.Drawing.Size(500, 31);
            this.gameRow.TabIndex = 1;
            this.gameRow.WrapContents = false;
            //
            // windowTitleLabel
            //
            this.windowTitleLabel.AutoSize = true;
            this.windowTitleLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.windowTitleLabel.Name = "windowTitleLabel";
            this.windowTitleLabel.Size = new System.Drawing.Size(43, 15);
            this.windowTitleLabel.TabIndex = 0;
            this.windowTitleLabel.Text = "창 제목";
            //
            // windowTitleBox
            //
            this.windowTitleBox.Name = "windowTitleBox";
            this.windowTitleBox.Size = new System.Drawing.Size(180, 23);
            this.windowTitleBox.TabIndex = 1;
            //
            // processNameLabel
            //
            this.processNameLabel.AutoSize = true;
            this.processNameLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.processNameLabel.Name = "processNameLabel";
            this.processNameLabel.Size = new System.Drawing.Size(55, 15);
            this.processNameLabel.TabIndex = 2;
            this.processNameLabel.Text = "프로세스";
            //
            // processNameBox
            //
            this.processNameBox.Name = "processNameBox";
            this.processNameBox.PlaceholderText = "비우면 확인 안 함";
            this.processNameBox.Size = new System.Drawing.Size(140, 23);
            this.processNameBox.TabIndex = 3;
            //
            // applyGameWindowButton
            //
            this.applyGameWindowButton.AutoSize = true;
            this.applyGameWindowButton.Name = "applyGameWindowButton";
            this.applyGameWindowButton.Size = new System.Drawing.Size(75, 25);
            this.applyGameWindowButton.TabIndex = 4;
            this.applyGameWindowButton.Text = "적용";
            this.applyGameWindowButton.Click += new System.EventHandler(this.ApplyGameWindowButton_Click);
            //
            // gameStatusLabel
            //
            this.gameStatusLabel.AutoSize = true;
            this.gameStatusLabel.ForeColor = System.Drawing.Color.DimGray;
            this.gameStatusLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gameStatusLabel.MaximumSize = new System.Drawing.Size(500, 0);
            this.gameStatusLabel.Name = "gameStatusLabel";
            this.gameStatusLabel.Size = new System.Drawing.Size(55, 15);
            this.gameStatusLabel.TabIndex = 2;
            this.gameStatusLabel.Text = "확인 중...";
            //
            // watchHeaderLabel
            //
            this.watchHeaderLabel.AutoSize = true;
            this.watchHeaderLabel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.watchHeaderLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.watchHeaderLabel.Name = "watchHeaderLabel";
            this.watchHeaderLabel.Size = new System.Drawing.Size(31, 15);
            this.watchHeaderLabel.TabIndex = 3;
            this.watchHeaderLabel.Text = "감시";
            //
            // watchRow
            //
            this.watchRow.AutoSize = true;
            this.watchRow.Controls.Add(this.toggleButton);
            this.watchRow.Controls.Add(this.calibrateButton);
            this.watchRow.Controls.Add(this.intervalCaptionLabel);
            this.watchRow.Controls.Add(this.intervalBar);
            this.watchRow.Controls.Add(this.intervalLabel);
            this.watchRow.Margin = new System.Windows.Forms.Padding(0);
            this.watchRow.Name = "watchRow";
            this.watchRow.Size = new System.Drawing.Size(450, 34);
            this.watchRow.TabIndex = 4;
            this.watchRow.WrapContents = false;
            //
            // toggleButton
            //
            this.toggleButton.AutoSize = true;
            this.toggleButton.Name = "toggleButton";
            this.toggleButton.Size = new System.Drawing.Size(75, 25);
            this.toggleButton.TabIndex = 0;
            this.toggleButton.Text = "중지 ■";
            this.toggleButton.Click += new System.EventHandler(this.ToggleButton_Click);
            //
            // calibrateButton
            //
            this.calibrateButton.AutoSize = true;
            this.calibrateButton.Name = "calibrateButton";
            this.calibrateButton.Size = new System.Drawing.Size(80, 25);
            this.calibrateButton.TabIndex = 1;
            this.calibrateButton.Text = "영역 설정...";
            this.calibrateButton.Click += new System.EventHandler(this.CalibrateButton_Click);
            //
            // intervalCaptionLabel
            //
            this.intervalCaptionLabel.AutoSize = true;
            this.intervalCaptionLabel.Margin = new System.Windows.Forms.Padding(12, 7, 3, 3);
            this.intervalCaptionLabel.Name = "intervalCaptionLabel";
            this.intervalCaptionLabel.Size = new System.Drawing.Size(55, 15);
            this.intervalCaptionLabel.TabIndex = 2;
            this.intervalCaptionLabel.Text = "검사 간격";
            //
            // intervalBar
            //
            this.intervalBar.AutoSize = false;
            this.intervalBar.Maximum = 20;
            this.intervalBar.Minimum = 2;
            this.intervalBar.Name = "intervalBar";
            this.intervalBar.Size = new System.Drawing.Size(140, 28);
            this.intervalBar.TabIndex = 3;
            this.intervalBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.intervalBar.Value = 5;
            this.intervalBar.ValueChanged += new System.EventHandler(this.IntervalBar_ValueChanged);
            //
            // intervalLabel
            //
            this.intervalLabel.AutoSize = true;
            this.intervalLabel.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
            this.intervalLabel.Name = "intervalLabel";
            this.intervalLabel.Size = new System.Drawing.Size(31, 15);
            this.intervalLabel.TabIndex = 4;
            this.intervalLabel.Text = "0.5 s";
            //
            // scanStatusLabel
            //
            this.scanStatusLabel.AutoSize = true;
            this.scanStatusLabel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.scanStatusLabel.ForeColor = System.Drawing.Color.DimGray;
            this.scanStatusLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.scanStatusLabel.MaximumSize = new System.Drawing.Size(500, 0);
            this.scanStatusLabel.Name = "scanStatusLabel";
            this.scanStatusLabel.Size = new System.Drawing.Size(55, 15);
            this.scanStatusLabel.TabIndex = 5;
            this.scanStatusLabel.Text = "대기 중";
            //
            // ocrHeaderLabel
            //
            this.ocrHeaderLabel.AutoSize = true;
            this.ocrHeaderLabel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.ocrHeaderLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.ocrHeaderLabel.Name = "ocrHeaderLabel";
            this.ocrHeaderLabel.Size = new System.Drawing.Size(55, 15);
            this.ocrHeaderLabel.TabIndex = 6;
            this.ocrHeaderLabel.Text = "인식 결과";
            //
            // ocrTextBox
            //
            this.ocrTextBox.Multiline = true;
            this.ocrTextBox.Name = "ocrTextBox";
            this.ocrTextBox.ReadOnly = true;
            this.ocrTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ocrTextBox.Size = new System.Drawing.Size(500, 150);
            this.ocrTextBox.TabIndex = 7;
            //
            // openOcrImageButton
            //
            this.openOcrImageButton.AutoSize = true;
            this.openOcrImageButton.Name = "openOcrImageButton";
            this.openOcrImageButton.Size = new System.Drawing.Size(105, 25);
            this.openOcrImageButton.TabIndex = 8;
            this.openOcrImageButton.Text = "OCR 이미지 보기";
            this.openOcrImageButton.Click += new System.EventHandler(this.OpenOcrImageButton_Click);
            //
            // filterHeaderLabel
            //
            this.filterHeaderLabel.AutoSize = true;
            this.filterHeaderLabel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.filterHeaderLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.filterHeaderLabel.Name = "filterHeaderLabel";
            this.filterHeaderLabel.Size = new System.Drawing.Size(43, 15);
            this.filterHeaderLabel.TabIndex = 9;
            this.filterHeaderLabel.Text = "거르기";
            //
            // keywordsCaptionLabel
            //
            this.keywordsCaptionLabel.AutoSize = true;
            this.keywordsCaptionLabel.Name = "keywordsCaptionLabel";
            this.keywordsCaptionLabel.Size = new System.Drawing.Size(300, 15);
            this.keywordsCaptionLabel.TabIndex = 10;
            this.keywordsCaptionLabel.Text = "직접 키워드 (쉼표로 구분, 하나라도 보이면 초기화 권장)";
            //
            // keywordsRow
            //
            this.keywordsRow.AutoSize = true;
            this.keywordsRow.Controls.Add(this.keywordsBox);
            this.keywordsRow.Controls.Add(this.applyKeywordsButton);
            this.keywordsRow.Margin = new System.Windows.Forms.Padding(0);
            this.keywordsRow.Name = "keywordsRow";
            this.keywordsRow.Size = new System.Drawing.Size(507, 31);
            this.keywordsRow.TabIndex = 11;
            this.keywordsRow.WrapContents = false;
            //
            // keywordsBox
            //
            this.keywordsBox.Name = "keywordsBox";
            this.keywordsBox.PlaceholderText = "예: 실명, 마비, 레이저";
            this.keywordsBox.Size = new System.Drawing.Size(420, 23);
            this.keywordsBox.TabIndex = 0;
            //
            // applyKeywordsButton
            //
            this.applyKeywordsButton.AutoSize = true;
            this.applyKeywordsButton.Name = "applyKeywordsButton";
            this.applyKeywordsButton.Size = new System.Drawing.Size(75, 25);
            this.applyKeywordsButton.TabIndex = 1;
            this.applyKeywordsButton.Text = "적용";
            this.applyKeywordsButton.Click += new System.EventHandler(this.ApplyKeywordsButton_Click);
            //
            // collectRow
            //
            this.collectRow.AutoSize = true;
            this.collectRow.Controls.Add(this.collectCheckBox);
            this.collectRow.Controls.Add(this.openPickerButton);
            this.collectRow.Margin = new System.Windows.Forms.Padding(0);
            this.collectRow.Name = "collectRow";
            this.collectRow.Size = new System.Drawing.Size(250, 31);
            this.collectRow.TabIndex = 12;
            this.collectRow.WrapContents = false;
            //
            // collectCheckBox
            //
            this.collectCheckBox.AutoSize = true;
            this.collectCheckBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.collectCheckBox.Name = "collectCheckBox";
            this.collectCheckBox.Size = new System.Drawing.Size(118, 19);
            this.collectCheckBox.TabIndex = 0;
            this.collectCheckBox.Text = "보는 옵션 모으기";
            this.collectCheckBox.CheckedChanged += new System.EventHandler(this.CollectCheckBox_CheckedChanged);
            //
            // openPickerButton
            //
            this.openPickerButton.AutoSize = true;
            this.openPickerButton.Name = "openPickerButton";
            this.openPickerButton.Size = new System.Drawing.Size(105, 25);
            this.openPickerButton.TabIndex = 1;
            this.openPickerButton.Text = "거를 옵션 선택...";
            this.openPickerButton.Click += new System.EventHandler(this.OpenPickerButton_Click);
            //
            // collectSummaryLabel
            //
            this.collectSummaryLabel.AutoSize = true;
            this.collectSummaryLabel.ForeColor = System.Drawing.Color.DimGray;
            this.collectSummaryLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.collectSummaryLabel.MaximumSize = new System.Drawing.Size(500, 0);
            this.collectSummaryLabel.Name = "collectSummaryLabel";
            this.collectSummaryLabel.Size = new System.Drawing.Size(150, 15);
            this.collectSummaryLabel.TabIndex = 13;
            this.collectSummaryLabel.Text = "모은 옵션 0개 · 거르는 옵션 0개";
            //
            // configHeaderLabel
            //
            this.configHeaderLabel.AutoSize = true;
            this.configHeaderLabel.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold);
            this.configHeaderLabel.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.configHeaderLabel.Name = "configHeaderLabel";
            this.configHeaderLabel.Size = new System.Drawing.Size(55, 15);
            this.configHeaderLabel.TabIndex = 14;
            this.configHeaderLabel.Text = "설정 파일";
            //
            // configRow
            //
            this.configRow.AutoSize = true;
            this.configRow.Controls.Add(this.openConfigButton);
            this.configRow.Controls.Add(this.reloadConfigButton);
            this.configRow.Margin = new System.Windows.Forms.Padding(0);
            this.configRow.Name = "configRow";
            this.configRow.Size = new System.Drawing.Size(200, 31);
            this.configRow.TabIndex = 15;
            this.configRow.WrapContents = false;
            //
            // openConfigButton
            //
            this.openConfigButton.AutoSize = true;
            this.openConfigButton.Name = "openConfigButton";
            this.openConfigButton.Size = new System.Drawing.Size(95, 25);
            this.openConfigButton.TabIndex = 0;
            this.openConfigButton.Text = "설정 파일 열기";
            this.openConfigButton.Click += new System.EventHandler(this.OpenConfigButton_Click);
            //
            // reloadConfigButton
            //
            this.reloadConfigButton.AutoSize = true;
            this.reloadConfigButton.Name = "reloadConfigButton";
            this.reloadConfigButton.Size = new System.Drawing.Size(95, 25);
            this.reloadConfigButton.TabIndex = 1;
            this.reloadConfigButton.Text = "다시 불러오기";
            this.reloadConfigButton.Click += new System.EventHandler(this.ReloadConfigButton_Click);
            //
            // noticeLabel
            //
            this.noticeLabel.AutoSize = true;
            this.noticeLabel.ForeColor = System.Drawing.Color.DimGray;
            this.noticeLabel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.noticeLabel.MaximumSize = new System.Drawing.Size(500, 0);
            this.noticeLabel.Name = "noticeLabel";
            this.noticeLabel.Size = new System.Drawing.Size(0, 15);
            this.noticeLabel.TabIndex = 16;
            //
            // scanTimer
            //
            this.scanTimer.Interval = 500;
            this.scanTimer.Tick += new System.EventHandler(this.ScanTimer_Tick);
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(530, 640);
            this.Controls.Add(this.rootPanel);
            this.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "맵 옵션 가드";
            this.rootPanel.ResumeLayout(false);
            this.rootPanel.PerformLayout();
            this.gameRow.ResumeLayout(false);
            this.gameRow.PerformLayout();
            this.watchRow.ResumeLayout(false);
            this.watchRow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.intervalBar)).EndInit();
            this.keywordsRow.ResumeLayout(false);
            this.keywordsRow.PerformLayout();
            this.collectRow.ResumeLayout(false);
            this.collectRow.PerformLayout();
            this.configRow.ResumeLayout(false);
            this.configRow.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel rootPanel;
        private System.Windows.Forms.Label gameHeaderLabel;
        private System.Windows.Forms.FlowLayoutPanel gameRow;
        private System.Windows.Forms.Label windowTitleLabel;
        private System.Windows.Forms.TextBox windowTitleBox;
        private System.Windows.Forms.Label processNameLabel;
        private System.Windows.Forms.TextBox processNameBox;
        private System.Windows.Forms.Button applyGameWindowButton;
        private System.Windows.Forms.Label gameStatusLabel;
        private System.Windows.Forms.Label watchHeaderLabel;
        private System.Windows.Forms.FlowLayoutPanel watchRow;
        private System.Windows.Forms.Button toggleButton;
        private System.Windows.Forms.Button calibrateButton;
        private System.Windows.Forms.Label intervalCaptionLabel;
        private System.Windows.Forms.TrackBar intervalBar;
        private System.Windows.Forms.Label intervalLabel;
        private System.Windows.Forms.Label scanStatusLabel;
        private System.Windows.Forms.Label ocrHeaderLabel;
        private System.Windows.Forms.TextBox ocrTextBox;
        private System.Windows.Forms.Button openOcrImageButton;
        private System.Windows.Forms.Label filterHeaderLabel;
        private System.Windows.Forms.Label keywordsCaptionLabel;
        private System.Windows.Forms.FlowLayoutPanel keywordsRow;
        private System.Windows.Forms.TextBox keywordsBox;
        private System.Windows.Forms.Button applyKeywordsButton;
        private System.Windows.Forms.FlowLayoutPanel collectRow;
        private System.Windows.Forms.CheckBox collectCheckBox;
        private System.Windows.Forms.Button openPickerButton;
        private System.Windows.Forms.Label collectSummaryLabel;
        private System.Windows.Forms.Label configHeaderLabel;
        private System.Windows.Forms.FlowLayoutPanel configRow;
        private System.Windows.Forms.Button openConfigButton;
        private System.Windows.Forms.Button reloadConfigButton;
        private System.Windows.Forms.Label noticeLabel;
        private System.Windows.Forms.Timer scanTimer;
    }
}
