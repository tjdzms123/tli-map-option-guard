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
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.searchLayout = new System.Windows.Forms.TableLayoutPanel();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.blockedOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.optionListView = new System.Windows.Forms.ListView();
            this.optionColumn = new System.Windows.Forms.ColumnHeader();
            this.summaryLabel = new System.Windows.Forms.Label();
            this.rootLayout.SuspendLayout();
            this.searchLayout.SuspendLayout();
            this.SuspendLayout();
            //
            // rootLayout
            //
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.searchLayout, 0, 0);
            this.rootLayout.Controls.Add(this.optionListView, 0, 1);
            this.rootLayout.Controls.Add(this.summaryLabel, 0, 2);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(8);
            this.rootLayout.RowCount = 3;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.rootLayout.Size = new System.Drawing.Size(944, 601);
            this.rootLayout.TabIndex = 0;
            //
            // searchLayout
            //
            this.searchLayout.AutoSize = true;
            this.searchLayout.ColumnCount = 2;
            this.searchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.searchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.searchLayout.Controls.Add(this.searchBox, 0, 0);
            this.searchLayout.Controls.Add(this.blockedOnlyCheckBox, 1, 0);
            this.searchLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchLayout.Location = new System.Drawing.Point(11, 11);
            this.searchLayout.Name = "searchLayout";
            this.searchLayout.RowCount = 1;
            this.searchLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.searchLayout.Size = new System.Drawing.Size(922, 31);
            this.searchLayout.TabIndex = 0;
            //
            // searchBox
            //
            this.searchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchBox.Name = "searchBox";
            this.searchBox.PlaceholderText = "검색 (띄어쓰기 무시)";
            this.searchBox.Size = new System.Drawing.Size(770, 25);
            this.searchBox.TabIndex = 0;
            this.searchBox.TextChanged += new System.EventHandler(this.SearchBox_TextChanged);
            //
            // blockedOnlyCheckBox
            //
            this.blockedOnlyCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.blockedOnlyCheckBox.AutoSize = true;
            this.blockedOnlyCheckBox.Name = "blockedOnlyCheckBox";
            this.blockedOnlyCheckBox.Size = new System.Drawing.Size(140, 23);
            this.blockedOnlyCheckBox.TabIndex = 1;
            this.blockedOnlyCheckBox.Text = "거르는 옵션만 보기";
            this.blockedOnlyCheckBox.CheckedChanged += new System.EventHandler(this.BlockedOnlyCheckBox_CheckedChanged);
            //
            // optionListView
            //
            this.optionListView.CheckBoxes = true;
            this.optionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.optionColumn});
            this.optionListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionListView.FullRowSelect = true;
            this.optionListView.GridLines = true;
            this.optionListView.Name = "optionListView";
            this.optionListView.Size = new System.Drawing.Size(922, 515);
            this.optionListView.TabIndex = 1;
            this.optionListView.UseCompatibleStateImageBehavior = false;
            this.optionListView.View = System.Windows.Forms.View.Details;
            this.optionListView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.OptionListView_ItemChecked);
            this.optionListView.Resize += new System.EventHandler(this.OptionListView_Resize);
            //
            // optionColumn
            //
            this.optionColumn.Text = "옵션 (체크하면 거름)";
            this.optionColumn.Width = 895;
            //
            // summaryLabel
            //
            this.summaryLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.summaryLabel.AutoSize = true;
            this.summaryLabel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.summaryLabel.Name = "summaryLabel";
            this.summaryLabel.Size = new System.Drawing.Size(0, 19);
            this.summaryLabel.TabIndex = 2;
            //
            // OptionPickerForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(944, 601);
            this.Controls.Add(this.rootLayout);
            this.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.MinimumSize = new System.Drawing.Size(560, 360);
            this.Name = "OptionPickerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "맵 옵션 가드 - 거를 옵션 선택";
            this.Activated += new System.EventHandler(this.OptionPickerForm_Activated);
            this.rootLayout.ResumeLayout(false);
            this.rootLayout.PerformLayout();
            this.searchLayout.ResumeLayout(false);
            this.searchLayout.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel searchLayout;
        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.CheckBox blockedOnlyCheckBox;
        private System.Windows.Forms.ListView optionListView;
        private System.Windows.Forms.ColumnHeader optionColumn;
        private System.Windows.Forms.Label summaryLabel;
    }
}
