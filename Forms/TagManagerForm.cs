using WebNavigator.Models;
using WebNavigator.Services;
using WebNavigator.Utils;

namespace WebNavigator.Forms;

public partial class TagManagerForm : Form
{
    private readonly AppConfig _config;
    private ListView _tagListView = null!;
    private TextBox _newTagTextBox = null!;
    private Button _addButton = null!;
    private Label _infoLabel = null!;

    public TagManagerForm(AppConfig config)
    {
        _config = config;
        InitializeComponent();
        LoadTags();
    }

    private void InitializeComponent()
    {
        this.Text = "skyの标签管理";
        this.Size = new Size(680, 560);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        this.Icon = ResourceHelper.GetEmbeddedIcon();

        var titleLabel = new Label
        {
            Text = "🏷️ 标签管理",
            Location = new Point(10, 15),
            Size = new Size(660, 40),
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 51, 51),
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false
        };

        _infoLabel = new Label
        {
            Text = "管理所有网站标签",
            Location = new Point(20, 60),
            Size = new Size(640, 25),
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = Color.FromArgb(153, 153, 153),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _tagListView = new ListView
        {
            Location = new Point(20, 95),
            Size = new Size(620, 230),
            FullRowSelect = true,
            GridLines = true,
            View = View.Details,
            BackColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 10F),
            OwnerDraw = true
        };
        _tagListView.Columns.Add("标签名称", 360);
        _tagListView.Columns.Add("使用次数", 120);
        _tagListView.Columns.Add("操作", 140);
        _tagListView.SelectedIndexChanged += TagListView_SelectedIndexChanged;
        _tagListView.DoubleClick += TagListView_DoubleClick;
        _tagListView.DrawColumnHeader += TagListView_DrawColumnHeader;
        _tagListView.DrawSubItem += TagListView_DrawSubItem;
        _tagListView.MouseClick += TagListView_MouseClick;

        var buttonPanel = new Panel
        {
            Location = new Point(20, 335),
            Size = new Size(620, 40),
            BackColor = Color.FromArgb(250, 250, 252)
        };

        var editButton = new Button
        {
            Text = "✏️ 编辑",
            Location = new Point(10, 5),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        editButton.FlatAppearance.BorderSize = 0;
        editButton.Click += (s, e) =>
        {
            if (_tagListView.SelectedItems.Count > 0)
            {
                var tag = _tagListView.SelectedItems[0].Tag?.ToString();
                if (!string.IsNullOrEmpty(tag))
                {
                    EditTag(tag);
                }
            }
            else
            {
                MessageBox.Show("请先选择一个标签", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };

        var deleteButton = new Button
        {
            Text = "🗑️ 删除",
            Location = new Point(110, 5),
            Size = new Size(90, 30),
            BackColor = Color.FromArgb(255, 77, 79),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        deleteButton.FlatAppearance.BorderSize = 0;
        deleteButton.Click += (s, e) =>
        {
            if (_tagListView.SelectedItems.Count > 0)
            {
                var tag = _tagListView.SelectedItems[0].Tag?.ToString();
                if (!string.IsNullOrEmpty(tag))
                {
                    DeleteTag(tag);
                }
            }
            else
            {
                MessageBox.Show("请先选择一个标签", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        };

        buttonPanel.Controls.Add(editButton);
        buttonPanel.Controls.Add(deleteButton);

        var addPanel = new Panel
        {
            Location = new Point(20, 390),
            Size = new Size(620, 50),
            BackColor = Color.FromArgb(250, 250, 252)
        };

        var addLabel = new Label
        {
            Text = "添加新标签:",
            Location = new Point(10, 12),
            Size = new Size(90, 25),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        _newTagTextBox = new TextBox
        {
            Location = new Point(105, 10),
            Size = new Size(420, 28),
            Font = new Font("Microsoft YaHei UI", 10F),
            BorderStyle = BorderStyle.FixedSingle
        };

        _addButton = new Button
        {
            Text = "添加",
            Location = new Point(535, 8),
            Size = new Size(70, 30),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        _addButton.FlatAppearance.BorderSize = 0;
        _addButton.Click += AddButton_Click;

        addPanel.Controls.Add(addLabel);
        addPanel.Controls.Add(_newTagTextBox);
        addPanel.Controls.Add(_addButton);

        var closeButton = new Button
        {
            Text = "关闭",
            Location = new Point(this.Width - 120, 500),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(102, 102, 102),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        closeButton.FlatAppearance.BorderSize = 0;
        closeButton.Click += (s, e) => this.Close();

        this.Controls.Add(titleLabel);
        this.Controls.Add(_infoLabel);
        this.Controls.Add(_tagListView);
        this.Controls.Add(buttonPanel);
        this.Controls.Add(addPanel);
        this.Controls.Add(closeButton);

        this.AcceptButton = _addButton;
    }

    private void LoadTags()
    {
        _tagListView.Items.Clear();

        // 获取所有网站使用的标签，并合并 _config.Tags 中的标签
        var websiteTags = _config.Websites
            .SelectMany(w => w.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var allTags = websiteTags
            .Union(_config.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(tag => new
            {
                Tag = tag,
                Count = _config.Websites.Count(w => w.Tags.Contains(tag))
            })
            .OrderBy(x => x.Tag);

        foreach (var tagInfo in allTags)
        {
            var item = new ListViewItem(tagInfo.Tag);
            item.SubItems.Add(tagInfo.Count.ToString());
            item.SubItems.Add("编辑 | 删除");
            item.Tag = tagInfo.Tag;
            _tagListView.Items.Add(item);
        }

        UpdateInfoLabel();
    }

    private void UpdateInfoLabel()
    {
        var totalTags = _config.Tags.Count;
        _infoLabel.Text = $"共 {totalTags} 个标签，{_config.Websites.Count} 个网站";
    }

    private void TagListView_SelectedIndexChanged(object? sender, EventArgs e)
    {
    }

    private void TagListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
    {
        e.DrawDefault = true;
    }

    private void TagListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
    {
        if (e.ColumnIndex == 2)
        {
            var subItemBounds = e.Bounds;
            var editWidth = (int)(subItemBounds.Width * 0.48);
            var deleteWidth = subItemBounds.Width - editWidth - 4;
            var font = e.Item?.Font ?? _tagListView.Font ?? System.Drawing.SystemFonts.DefaultFont;

            using var editBrush = new SolidBrush(Color.FromArgb(51, 102, 255));
            using var deleteBrush = new SolidBrush(Color.FromArgb(217, 83, 79));

            e.Graphics.FillRectangle(editBrush, subItemBounds.X + 2, subItemBounds.Y + 2, editWidth, subItemBounds.Height - 4);
            e.Graphics.DrawString("编辑", font, Brushes.White, subItemBounds.X + 12, subItemBounds.Y + 4);

            e.Graphics.FillRectangle(deleteBrush, subItemBounds.X + editWidth + 6, subItemBounds.Y + 2, deleteWidth, subItemBounds.Height - 4);
            e.Graphics.DrawString("删除", font, Brushes.White, subItemBounds.X + editWidth + 16, subItemBounds.Y + 4);
        }
        else
        {
            e.DrawDefault = true;
        }
    }

    private void TagListView_DoubleClick(object? sender, EventArgs e)
    {
        if (_tagListView.SelectedItems.Count > 0)
        {
            var tag = _tagListView.SelectedItems[0].Tag?.ToString();
            if (!string.IsNullOrEmpty(tag))
            {
                EditTag(tag);
            }
        }
    }

    private void TagListView_MouseClick(object? sender, MouseEventArgs e)
    {
        var hitTest = _tagListView.HitTest(e.X, e.Y);
        if (hitTest.Item != null && hitTest.SubItem != null)
        {
            var tag = hitTest.Item.Tag?.ToString();
            if (!string.IsNullOrEmpty(tag) && hitTest.Item.SubItems.IndexOf(hitTest.SubItem) == 2)
            {
                var subItemBounds = hitTest.SubItem.Bounds;
                var editWidth = (int)(subItemBounds.Width * 0.45);

                if (e.X - hitTest.SubItem.Bounds.X < editWidth)
                {
                    EditTag(tag);
                }
                else
                {
                    DeleteTag(tag);
                }
            }
        }
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        var newTag = _newTagTextBox.Text.Trim();

        if (string.IsNullOrEmpty(newTag))
        {
            MessageBox.Show("请输入标签名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _newTagTextBox.Focus();
            return;
        }

        // 检查标签是否已存在（包括 _config.Tags 和网站中的标签）
        var allExistingTags = _config.Tags
            .Union(_config.Websites.SelectMany(w => w.Tags))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        if (allExistingTags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
        {
            MessageBox.Show("该标签已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _config.Tags.Add(newTag);
        ConfigService.SaveConfig(_config);
        _newTagTextBox.Clear();
        LoadTags();
    }

    private void EditTag(string oldTag)
    {
        var form = new TagEditForm(oldTag);
        if (form.ShowDialog() == DialogResult.OK)
        {
            var newTag = form.NewTag;
            if (oldTag == newTag) return;

            // 检查标签是否已存在（包括 _config.Tags 和网站中的标签）
            var allExistingTags = _config.Tags
                .Union(_config.Websites.SelectMany(w => w.Tags))
                .Distinct(StringComparer.OrdinalIgnoreCase);
            if (allExistingTags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
            {
                MessageBox.Show("该标签名称已存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var websiteCount = _config.Websites.Count(w => w.Tags.Contains(oldTag));

            var result = MessageBox.Show(
                $"确定要将「{oldTag}」重命名为「{newTag}」吗？\n\n这将影响 {websiteCount} 个网站。",
                "确认重命名",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                var tagIndex = _config.Tags.FindIndex(t => t == oldTag);
                if (tagIndex >= 0)
                {
                    _config.Tags[tagIndex] = newTag;
                }

                foreach (var website in _config.Websites.Where(w => w.Tags.Contains(oldTag)))
                {
                    var index = website.Tags.FindIndex(t => t == oldTag);
                    if (index >= 0)
                    {
                        website.Tags[index] = newTag;
                    }
                    website.UpdatedAt = DateTime.Now;
                }

                ConfigService.SaveConfig(_config);
                MessageBox.Show("标签重命名成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTags();
            }
        }
    }

    public void DeleteTag(string tag)
    {
        var websiteCount = _config.Websites.Count(w => w.Tags.Contains(tag));

        var result = MessageBox.Show(
            websiteCount > 0
                ? $"标签「{tag}」正在被 {websiteCount} 个网站使用。\n\n确定要删除该标签吗？这将从所有网站中移除此标签。"
                : $"确定要删除标签「{tag}」吗？",
            "确认删除",
            MessageBoxButtons.YesNo,
            websiteCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            _config.Tags.Remove(tag);

            foreach (var website in _config.Websites.Where(w => w.Tags.Contains(tag)))
            {
                website.Tags.Remove(tag);
                website.UpdatedAt = DateTime.Now;
            }

            ConfigService.SaveConfig(_config);
            MessageBox.Show(websiteCount > 0
                ? "标签已从所有网站中移除！"
                : "标签已删除！",
                "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadTags();
        }
    }
}

public class TagEditForm : Form
{
    private TextBox _nameTextBox = null!;
    private readonly string _oldTag;

    public string NewTag => _nameTextBox.Text.Trim();

    public TagEditForm(string oldTag)
    {
        _oldTag = oldTag;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "编辑标签";
        this.Size = new Size(400, 180);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        this.Icon = ResourceHelper.GetEmbeddedIcon();

        var label = new Label
        {
            Text = "新标签名称:",
            Location = new Point(20, 30),
            Size = new Size(100, 25),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        _nameTextBox = new TextBox
        {
            Location = new Point(20, 60),
            Size = new Size(340, 28),
            Font = new Font("Microsoft YaHei UI", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            Text = _oldTag
        };

        var saveButton = new Button
        {
            Text = "保存",
            Location = new Point(this.Width - 220, 110),
            Size = new Size(90, 32),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
            {
                MessageBox.Show("请输入标签名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.DialogResult = DialogResult.OK;
        };

        var cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(this.Width - 110, 110),
            Size = new Size(90, 32),
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(102, 102, 102),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.Add(label);
        this.Controls.Add(_nameTextBox);
        this.Controls.Add(saveButton);
        this.Controls.Add(cancelButton);

        this.AcceptButton = saveButton;
        this.CancelButton = cancelButton;
    }
}

public class WebsiteSelectForm : Form
{
    private readonly List<WebsiteItem> _websites;
    private readonly string _tag;
    private CheckedListBox _websiteListBox = null!;

    public WebsiteSelectForm(List<WebsiteItem> websites, string tag)
    {
        _websites = websites;
        _tag = tag;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "选择网站";
        this.Size = new Size(500, 450);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        this.Icon = ResourceHelper.GetEmbeddedIcon();

        var titleLabel = new Label
        {
            Text = $"选择要添加标签「{_tag}」的网站",
            Location = new Point(20, 20),
            Size = new Size(440, 30),
            Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 51, 51),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var tipLabel = new Label
        {
            Text = "勾选要添加标签的网站",
            Location = new Point(20, 55),
            Size = new Size(440, 20),
            Font = new Font("Microsoft YaHei UI", 9F),
            ForeColor = Color.FromArgb(153, 153, 153),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _websiteListBox = new CheckedListBox
        {
            Location = new Point(20, 85),
            Size = new Size(440, 250),
            CheckOnClick = true,
            BackColor = Color.White,
            Font = new Font("Microsoft YaHei UI", 10F),
            BorderStyle = BorderStyle.FixedSingle
        };

        foreach (var website in _websites)
        {
            _websiteListBox.Items.Add(website.Name, false);
        }

        var selectAllButton = new Button
        {
            Text = "全选",
            Location = new Point(20, 345),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(51, 51, 51),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        selectAllButton.FlatAppearance.BorderSize = 0;
        selectAllButton.Click += (s, e) =>
        {
            for (int i = 0; i < _websiteListBox.Items.Count; i++)
            {
                _websiteListBox.SetItemChecked(i, true);
            }
        };

        var deselectAllButton = new Button
        {
            Text = "取消全选",
            Location = new Point(130, 345),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(51, 51, 51),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        deselectAllButton.FlatAppearance.BorderSize = 0;
        deselectAllButton.Click += (s, e) =>
        {
            for (int i = 0; i < _websiteListBox.Items.Count; i++)
            {
                _websiteListBox.SetItemChecked(i, false);
            }
        };

        var confirmButton = new Button
        {
            Text = "确认添加",
            Location = new Point(this.Width - 220, 380),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        confirmButton.FlatAppearance.BorderSize = 0;
        confirmButton.Click += ConfirmButton_Click;

        var cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(this.Width - 110, 380),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(102, 102, 102),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.Add(titleLabel);
        this.Controls.Add(tipLabel);
        this.Controls.Add(_websiteListBox);
        this.Controls.Add(selectAllButton);
        this.Controls.Add(deselectAllButton);
        this.Controls.Add(confirmButton);
        this.Controls.Add(cancelButton);
    }

    private void ConfirmButton_Click(object? sender, EventArgs e)
    {
        var checkedIndices = _websiteListBox.CheckedIndices;
        if (checkedIndices.Count == 0)
        {
            MessageBox.Show("请至少选择一个网站", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (int index in checkedIndices)
        {
            _websites[index].Tags.Add(_tag);
            _websites[index].UpdatedAt = DateTime.Now;
        }

        this.DialogResult = DialogResult.OK;
    }
}
