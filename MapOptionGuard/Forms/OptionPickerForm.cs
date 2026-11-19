namespace MapOptionGuard;

/// <summary>
/// 지금까지 모은 옵션을 검색하고, 체크한 옵션을 '거를 옵션'으로 지정하는 창.
/// 체크를 바꾸거나 옵션을 지우면 바로 수집 파일에 저장되고 다음 검사부터 적용된다.
/// 컨트롤 배치는 OptionPickerForm.Designer.cs(VS 디자이너)에서, 동작은 이 파일에서 다룬다.
/// </summary>
internal sealed partial class OptionPickerForm : Form
{
    private readonly OptionCollector _collector;
    // 설정을 다시 불러오면 바뀔 수 있어 값 대신 읽는 방법을 받는다
    private readonly Func<double> _getSameOptionSimilarity;
    private bool _refreshing;

    public OptionPickerForm(OptionCollector collector, Func<double> getSameOptionSimilarity)
    {
        InitializeComponent();
        _collector = collector;
        _getSameOptionSimilarity = getSameOptionSimilarity;
    }

    // 게임을 하다 돌아오면 그 사이 새로 모인 옵션도 보이도록 다시 그린다
    private void OptionPickerForm_Activated(object? sender, EventArgs e) => RefreshList();

    private void SearchBox_TextChanged(object? sender, EventArgs e) => RefreshList();

    private void BlockedOnlyCheckBox_CheckedChanged(object? sender, EventArgs e) => RefreshList();

    // 열이 하나뿐이라 창 너비에 맞춰 옵션 열을 늘린다
    private void OptionListView_Resize(object? sender, EventArgs e) =>
        optionColumn.Width = Math.Max(100, optionListView.ClientSize.Width);

    private void OptionListView_ItemChecked(object? sender, ItemCheckedEventArgs e)
    {
        if (_refreshing || e.Item.Tag is not SeenOption option || option.Blocked == e.Item.Checked)
        {
            return;
        }

        option.Blocked = e.Item.Checked;
        TrySave();
        UpdateSummary();
    }

    // OCR이 틀리게 읽은 옵션 글자를 바로잡는다. 고친 글자와 비슷한 다른 옵션은 이 옵션 하나로 합친다.
    private void OptionListView_AfterLabelEdit(object? sender, LabelEditEventArgs e)
    {
        var text = e.Label?.Trim();
        if (text is null || optionListView.Items[e.Item].Tag is not SeenOption option)
        {
            // Esc로 취소했거나 글자를 바꾸지 않았다
            return;
        }

        if (KeywordMatcher.RemoveWhitespace(text).Length == 0)
        {
            e.CancelEdit = true;
            ShowInfoAfterEdit("옵션 글자를 비울 수는 없습니다. 옵션을 없애려면 '선택한 옵션 지우기'를 누르세요.");
            return;
        }

        option.Text = text;
        int mergedCount = _collector.MergeInto(option, _getSameOptionSimilarity());
        TrySave();
        // 이 이벤트 안에서 목록 항목을 지우면 편집 상자가 꼬이므로, 다시 그리기는 편집이 끝난 뒤로 미룬다
        BeginInvoke(new Action(RefreshList));
        if (mergedCount > 0)
        {
            ShowInfoAfterEdit($"고친 글자와 비슷한 옵션 {mergedCount}개를 이 옵션으로 합쳤습니다.");
        }
    }

    private void ShowInfoAfterEdit(string message) => BeginInvoke(new Action(() => ShowInfo(message)));

    private void OptionListView_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F2)
        {
            optionListView.FocusedItem?.BeginEdit();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Delete)
        {
            RemoveSelectedOptions();
            e.Handled = true;
        }
    }

    private void RemoveSelectedButton_Click(object? sender, EventArgs e) => RemoveSelectedOptions();

    private void ClearCollectedButton_Click(object? sender, EventArgs e)
    {
        int unblockedCount = _collector.Options.Count(o => !o.Blocked);
        if (unblockedCount == 0)
        {
            ShowInfo("비울 옵션이 없습니다. 거르기로 고른 옵션은 지우지 않습니다.");
            return;
        }

        if (!Confirm($"거르기로 고르지 않은 옵션 {unblockedCount}개를 모두 지울까요?\n거르기로 고른 옵션은 남겨 둡니다."))
        {
            return;
        }

        _collector.RemoveUnblocked(_ => true);
        TrySave();
        RefreshList();
    }

    private void RemoveSelectedOptions()
    {
        var selected = optionListView.SelectedItems.Cast<ListViewItem>()
            .Select(item => item.Tag)
            .OfType<SeenOption>()
            .ToList();
        if (selected.Count == 0)
        {
            ShowInfo("지울 옵션을 목록에서 먼저 선택해 주세요. Ctrl이나 Shift를 누른 채 클릭하면 여러 개를 고를 수 있습니다.");
            return;
        }

        var question = $"선택한 옵션 {selected.Count}개를 목록에서 지울까요?";
        int blockedCount = selected.Count(o => o.Blocked);
        if (blockedCount > 0)
        {
            question += $"\n그중 거르기로 고른 옵션 {blockedCount}개는 더 이상 거르지 않습니다.";
        }

        if (!Confirm(question + "\n지운 옵션도 다시 보이면 새로 모입니다."))
        {
            return;
        }

        _collector.Remove(selected);
        TrySave();
        RefreshList();
    }

    // 저장에 실패해도 창은 계속 쓸 수 있도록 알리기만 한다
    private void TrySave()
    {
        try
        {
            _collector.Save();
        }
        catch (Exception ex)
        {
            MessageBox.Show("저장하지 못했습니다.\n" + ex.Message, Program.AppTitle, MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    // 지우기는 되돌릴 수 없으므로 기본 선택을 '아니요'로 둔다
    private bool Confirm(string question) =>
        MessageBox.Show(this, question, Program.AppTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    private void ShowInfo(string message) =>
        MessageBox.Show(this, message, Program.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void RefreshList()
    {
        var query = KeywordMatcher.RemoveWhitespace(searchBox.Text);
        var options = _collector.Options
            .Where(o => !blockedOnlyCheckBox.Checked || o.Blocked)
            .Where(o => query.Length == 0 || o.CompactText.Contains(query, StringComparison.OrdinalIgnoreCase))
            // 본 횟수는 표시하지 않지만, 자주 나오는 옵션이 위에 오도록 정렬에는 쓴다
            .OrderByDescending(o => o.Count)
            .ThenBy(o => o.Text, StringComparer.Ordinal);

        _refreshing = true;
        optionListView.BeginUpdate();
        try
        {
            optionListView.Items.Clear();
            foreach (var option in options)
            {
                optionListView.Items.Add(new ListViewItem(option.Text) { Checked = option.Blocked, Tag = option });
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
            : $"모은 옵션 {_collector.Count}개 · 거르는 옵션 {_collector.Options.Count(o => o.Blocked)}개 · 체크하면 바로 저장되고 다음 검사부터 적용됩니다. 글자를 고치려면 행을 고르고 F2를 누르세요.";
    }
}
