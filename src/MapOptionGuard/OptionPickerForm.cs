namespace MapOptionGuard;

/// <summary>
/// 지금까지 모은 옵션을 검색하고, 체크한 옵션을 '거를 옵션'으로 지정하는 창.
/// 체크를 바꾸면 바로 수집 파일에 저장되고 다음 검사부터 적용된다.
/// 컨트롤 배치는 OptionPickerForm.Designer.cs(VS 디자이너)에서, 동작은 이 파일에서 다룬다.
/// </summary>
internal sealed partial class OptionPickerForm : Form
{
    private readonly OptionCollector _collector;
    private bool _refreshing;

    public OptionPickerForm(OptionCollector collector)
    {
        InitializeComponent();
        _collector = collector;
    }

    // 게임을 하다 돌아오면 그 사이 새로 모인 옵션도 보이도록 다시 그린다
    private void OptionPickerForm_Activated(object? sender, EventArgs e) => RefreshList();

    private void SearchBox_TextChanged(object? sender, EventArgs e) => RefreshList();

    private void BlockedOnlyCheckBox_CheckedChanged(object? sender, EventArgs e) => RefreshList();

    private void OptionListView_ItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (_refreshing || e.Item.Tag is not SeenOption option || option.Blocked == e.Item.Checked)
        {
            return;
        }

        option.Blocked = e.Item.Checked;
        try
        {
            _collector.Save();
        }
        catch (Exception ex)
        {
            MessageBox.Show("저장하지 못했습니다.\n" + ex.Message, Program.AppTitle, MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        UpdateSummary();
    }

    private void RefreshList()
    {
        var query = KeywordMatcher.RemoveWhitespace(searchBox.Text);
        var options = _collector.Options
            .Where(o => !blockedOnlyCheckBox.Checked || o.Blocked)
            .Where(o => query.Length == 0 || o.CompactText.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(o => o.Count)
            .ThenBy(o => o.Text, StringComparer.Ordinal);

        _refreshing = true;
        optionListView.BeginUpdate();
        try
        {
            optionListView.Items.Clear();
            foreach (var option in options)
            {
                var item = new ListViewItem(option.Text) { Checked = option.Blocked, Tag = option };
                item.SubItems.Add(option.Count.ToString());
                item.SubItems.Add(option.LastSeen);
                optionListView.Items.Add(item);
            }
        }
        finally
        {
            optionListView.EndUpdate();
            _refreshing = false;
        }

        UpdateSummary();
    }

    private void UpdateSummary()
    {
        summaryLabel.Text = _collector.Count == 0
            ? "아직 모은 옵션이 없습니다. 게임에서 맵 기본 옵션 창을 띄우면 자동으로 모입니다."
            : $"모은 옵션 {_collector.Count}개 · 거르는 옵션 {_collector.Options.Count(o => o.Blocked)}개 · 체크하면 바로 저장되고 다음 검사부터 적용됩니다.";
    }
}
