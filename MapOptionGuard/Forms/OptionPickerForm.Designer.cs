#nullable disable

namespace MapOptionGuard
{
    partial class OptionPickerForm
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
            rootLayout = new TableLayoutPanel();
            searchLayout = new TableLayoutPanel();
            searchBox = new TextBox();
            blockedOnlyCheckBox = new CheckBox();
            removeSelectedButton = new Button();
            clearCollectedButton = new Button();
            optionListView = new ListView();
            optionColumn = new ColumnHeader();
            summaryLabel = new Label();
            rootLayout.SuspendLayout();
            searchLayout.SuspendLayout();
            SuspendLayout();
            // 
            // rootLayout
            // 
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.Controls.Add(searchLayout, 0, 0);
            rootLayout.Controls.Add(optionListView, 0, 1);
            rootLayout.Controls.Add(summaryLabel, 0, 2);
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.Location = new Point(0, 0);
            rootLayout.Name = "rootLayout";
            rootLayout.Padding = new Padding(8);
            rootLayout.RowCount = 3;
            rootLayout.RowStyles.Add(new RowStyle());
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle());
            rootLayout.Size = new Size(944, 601);
            rootLayout.TabIndex = 0;
            // 
            // searchLayout
            // 
            searchLayout.AutoSize = true;
            searchLayout.ColumnCount = 4;
            searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            searchLayout.ColumnStyles.Add(new ColumnStyle());
            searchLayout.ColumnStyles.Add(new ColumnStyle());
            searchLayout.ColumnStyles.Add(new ColumnStyle());
            searchLayout.Controls.Add(searchBox, 0, 0);
            searchLayout.Controls.Add(blockedOnlyCheckBox, 1, 0);
            searchLayout.Controls.Add(removeSelectedButton, 2, 0);
            searchLayout.Controls.Add(clearCollectedButton, 3, 0);
            searchLayout.Dock = DockStyle.Fill;
            searchLayout.Location = new Point(11, 11);
            searchLayout.Name = "searchLayout";
            searchLayout.RowCount = 1;
            searchLayout.RowStyles.Add(new RowStyle());
            searchLayout.Size = new Size(922, 35);
            searchLayout.TabIndex = 0;
            // 
            // searchBox
            // 
            searchBox.Dock = DockStyle.Fill;
            searchBox.Location = new Point(3, 3);
            searchBox.Name = "searchBox";
            searchBox.PlaceholderText = "검색 (띄어쓰기 무시)";
            searchBox.Size = new Size(480, 25);
            searchBox.TabIndex = 0;
            searchBox.TextChanged += SearchBox_TextChanged;
            // 
            // blockedOnlyCheckBox
            // 
            blockedOnlyCheckBox.Anchor = AnchorStyles.Left;
            blockedOnlyCheckBox.AutoSize = true;
            blockedOnlyCheckBox.Location = new Point(489, 6);
            blockedOnlyCheckBox.Name = "blockedOnlyCheckBox";
            blockedOnlyCheckBox.Size = new Size(150, 23);
            blockedOnlyCheckBox.TabIndex = 1;
            blockedOnlyCheckBox.Text = "거르는 옵션만 보기";
            blockedOnlyCheckBox.CheckedChanged += BlockedOnlyCheckBox_CheckedChanged;
            // 
            // removeSelectedButton
            // 
            removeSelectedButton.AutoSize = true;
            removeSelectedButton.Location = new Point(645, 3);
            removeSelectedButton.Name = "removeSelectedButton";
            removeSelectedButton.Size = new Size(141, 29);
            removeSelectedButton.TabIndex = 2;
            removeSelectedButton.Text = "선택한 옵션 지우기";
            removeSelectedButton.Click += RemoveSelectedButton_Click;
            // 
            // clearCollectedButton
            // 
            clearCollectedButton.AutoSize = true;
            clearCollectedButton.Location = new Point(792, 3);
            clearCollectedButton.Name = "clearCollectedButton";
            clearCollectedButton.Size = new Size(127, 29);
            clearCollectedButton.TabIndex = 3;
            clearCollectedButton.Text = "모은 옵션 비우기";
            clearCollectedButton.Click += ClearCollectedButton_Click;
            // 
            // optionListView
            // 
            optionListView.CheckBoxes = true;
            optionListView.Columns.AddRange(new ColumnHeader[] { optionColumn });
            optionListView.Dock = DockStyle.Fill;
            optionListView.FullRowSelect = true;
            optionListView.GridLines = true;
            optionListView.LabelEdit = true;
            optionListView.Location = new Point(11, 52);
            optionListView.Name = "optionListView";
            optionListView.Size = new Size(922, 510);
            optionListView.TabIndex = 1;
            optionListView.UseCompatibleStateImageBehavior = false;
            optionListView.View = View.Details;
            optionListView.AfterLabelEdit += OptionListView_AfterLabelEdit;
            optionListView.ItemChecked += OptionListView_ItemChecked;
            optionListView.KeyDown += OptionListView_KeyDown;
            optionListView.Resize += OptionListView_Resize;
            // 
            // optionColumn
            // 
            optionColumn.Text = "옵션 (체크하면 거름)";
            optionColumn.Width = 895;
            // 
            // summaryLabel
            // 
            summaryLabel.Anchor = AnchorStyles.Left;
            summaryLabel.AutoSize = true;
            summaryLabel.Location = new Point(11, 571);
            summaryLabel.Margin = new Padding(3, 6, 3, 3);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.Size = new Size(0, 19);
            summaryLabel.TabIndex = 2;
            // 
            // OptionPickerForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(944, 601);
            Controls.Add(rootLayout);
            Font = new Font("맑은 고딕", 10F);
            MinimumSize = new Size(560, 360);
            Name = "OptionPickerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "맵 옵션 가드 - 거를 옵션 선택";
            Activated += OptionPickerForm_Activated;
            rootLayout.ResumeLayout(false);
            rootLayout.PerformLayout();
            searchLayout.ResumeLayout(false);
            searchLayout.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel searchLayout;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.CheckBox blockedOnlyCheckBox;
        private System.Windows.Forms.Button removeSelectedButton;
        private System.Windows.Forms.Button clearCollectedButton;
        private System.Windows.Forms.ListView optionListView;
        private System.Windows.Forms.ColumnHeader optionColumn;
        private System.Windows.Forms.Label summaryLabel;
    }
}
