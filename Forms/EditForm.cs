using WebNavigator.Models;
using WebNavigator.Services;

namespace WebNavigator.Forms;

public partial class EditForm : Form
{
    private readonly WebsiteItem? _original;
    private TextBox _nameTextBox = null!;
    private TextBox _urlTextBox = null!;
    private TextBox _descTextBox = null!;
    private TextBox _tagInputTextBox = null!;
    private ComboBox _tagComboBox = null!;
    private Panel _tagContainer = null!;
    private PictureBox _logoPreview = null!;
    private string? _tempLogoPath = null;
    private List<string> _tags = [];
    private readonly List<string> _allTags = [];

    public WebsiteItem Website { get; private set; } = null!;

    public EditForm(WebsiteItem? original)
    {
        _original = original;
        Website = original != null ? new WebsiteItem
        {
            Id = original.Id,
            Name = original.Name,
            Url = original.Url,
            LogoPath = original.LogoPath,
            Description = original.Description,
            Tags = [.. original.Tags],
            CreatedAt = original.CreatedAt,
            UpdatedAt = original.UpdatedAt
        } : new WebsiteItem();

        var config = ConfigService.LoadConfig();
        _allTags = [.. config.Tags.OrderBy(t => t)];

        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = _original != null ? "编辑网站" : "添加网站";
        this.Size = new Size(550, 760);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.White;

        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico");
        if (File.Exists(iconPath))
        {
            this.Icon = new Icon(iconPath);
        }

        var padding = 30;
        var y = padding;
        var labelWidth = 80;
        var inputWidth = 320;

        var nameLabel = new Label
        {
            Text = "网站名称",
            Location = new Point(padding, y),
            Size = new Size(labelWidth, 25),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        _nameTextBox = new TextBox
        {
            Location = new Point(padding + labelWidth + 10, y),
            Size = new Size(inputWidth, 30),
            Font = new Font("Microsoft YaHei UI", 10F),
            BorderStyle = BorderStyle.FixedSingle
        };

        y += 50;

        var urlLabel = new Label
        {
            Text = "网站地址",
            Location = new Point(padding, y),
            Size = new Size(labelWidth, 25),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        _urlTextBox = new TextBox
        {
            Location = new Point(padding + labelWidth + 10, y),
            Size = new Size(inputWidth, 30),
            Font = new Font("Microsoft YaHei UI", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "https://example.com"
        };

        y += 50;

        var logoLabel = new Label
        {
            Text = "Logo",
            Location = new Point(padding, y),
            Size = new Size(labelWidth, 25),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        var browseButton = new Button
        {
            Text = "浏览",
            Location = new Point(padding + labelWidth + 10, y),
            Size = new Size(80, 30),
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(51, 51, 51),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        browseButton.FlatAppearance.BorderSize = 0;
        browseButton.Click += BrowseButton_Click;

        y += 50;

        _logoPreview = new PictureBox
        {
            Location = new Point(padding + labelWidth + 10, y),
            Size = new Size(100, 100),
            BackColor = Color.FromArgb(245, 245, 245),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        y += 120;

        var descLabel = new Label
        {
            Text = "描述",
            Location = new Point(padding, y),
            Size = new Size(labelWidth, 25),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        _descTextBox = new TextBox
        {
            Location = new Point(padding + labelWidth + 10, y),
            Size = new Size(inputWidth, 80),
            Font = new Font("Microsoft YaHei UI", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical
        };

        y += 110;

        var tagLabel = new Label
        {
            Text = "标签",
            Location = new Point(padding, y),
            Size = new Size(labelWidth, 25),
            Font = new Font("Microsoft YaHei UI", 10F)
        };

        _tagComboBox = new ComboBox
        {
            Location = new Point(padding + labelWidth + 10, y),
            Size = new Size(200, 30),
            Font = new Font("Microsoft YaHei UI", 10F),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Tag"
        };
        _tagComboBox.Items.Add(new { Tag = "选择已有标签..." });
        foreach (var tag in _allTags)
        {
            _tagComboBox.Items.Add(new { Tag = tag });
        }
        _tagComboBox.SelectedIndex = 0;
        _tagComboBox.SelectedIndexChanged += TagComboBox_SelectedIndexChanged;

        y += 35;

        _tagInputTextBox = new TextBox
        {
            Location = new Point(padding + labelWidth + 10, y),
            Size = new Size(200, 30),
            Font = new Font("Microsoft YaHei UI", 10F),
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "或输入新标签后按回车"
        };
        _tagInputTextBox.KeyDown += TagInputTextBox_KeyDown;

        var addTagButton = new Button
        {
            Text = "添加",
            Location = new Point(padding + labelWidth + 10 + 210, y),
            Size = new Size(60, 30),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        addTagButton.FlatAppearance.BorderSize = 0;
        addTagButton.Click += AddTagButton_Click;

        y += 40;

        _tagContainer = new Panel
        {
            Location = new Point(padding + labelWidth + 10, y),
            Size = new Size(370, 80),
            AutoScroll = true,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(250, 250, 250)
        };

        y += 100;

        var saveButton = new Button
        {
            Text = "保存",
            Location = new Point(this.Width - 150, y),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += SaveButton_Click;

        var cancelButton = new Button
        {
            Text = "取消",
            Location = new Point(this.Width - 270, y),
            Size = new Size(100, 35),
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(102, 102, 102),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F)
        };
        cancelButton.FlatAppearance.BorderSize = 0;
        cancelButton.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

        this.Controls.Add(nameLabel);
        this.Controls.Add(_nameTextBox);
        this.Controls.Add(urlLabel);
        this.Controls.Add(_urlTextBox);
        this.Controls.Add(logoLabel);
        this.Controls.Add(browseButton);
        this.Controls.Add(_logoPreview);
        this.Controls.Add(tagLabel);
        this.Controls.Add(_tagComboBox);
        this.Controls.Add(_tagInputTextBox);
        this.Controls.Add(addTagButton);
        this.Controls.Add(_tagContainer);
        this.Controls.Add(descLabel);
        this.Controls.Add(_descTextBox);
        this.Controls.Add(saveButton);
        this.Controls.Add(cancelButton);

        this.AcceptButton = saveButton;
        this.CancelButton = cancelButton;
    }

    private void LoadData()
    {
        _nameTextBox.Text = Website.Name;
        _urlTextBox.Text = Website.Url;
        _descTextBox.Text = Website.Description;
        LoadLogoPreview(Website.LogoPath);
        _tags = [.. Website.Tags];
        RenderTags();
    }

    private void RenderTags()
    {
        _tagContainer.Controls.Clear();
        var x = 5;
        var y = 5;

        foreach (var tag in _tags)
        {
            var tagPanel = CreateTagControl(tag, x, y);
            _tagContainer.Controls.Add(tagPanel);

            x += tagPanel.Width + 5;
            if (x + 80 > _tagContainer.Width)
            {
                x = 5;
                y += 30;
            }
        }
    }

    private Panel CreateTagControl(string tag, int x, int y)
    {
        var panel = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(0, 0),
            BackColor = Color.Transparent
        };

        var tagLabel = new Label
        {
            Text = tag,
            Location = new Point(0, 0),
            AutoSize = false,
            Size = new Size(TextRenderer.MeasureText(tag, new Font("Microsoft YaHei UI", 9F)).Width + 20, 25),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft YaHei UI", 9F),
            Cursor = Cursors.Hand
        };
        tagLabel.Click += (s, e) => RemoveTag(tag);

        var removeButton = new Button
        {
            Text = "×",
            Location = new Point(tagLabel.Width - 15, 2),
            Size = new Size(18, 18),
            BackColor = Color.Transparent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
        };
        removeButton.FlatAppearance.BorderSize = 0;
        removeButton.Click += (s, e) => RemoveTag(tag);

        panel.Controls.Add(tagLabel);
        panel.Controls.Add(removeButton);
        panel.Size = new Size(tagLabel.Width + 5, 28);

        return panel;
    }

    private void RemoveTag(string tag)
    {
        _tags.Remove(tag);
        RenderTags();
    }

    private void AddTagButton_Click(object? sender, EventArgs e)
    {
        AddTag();
    }

    private void TagInputTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            AddTag();
        }
    }

    private void AddTag()
    {
        var tag = _tagInputTextBox.Text.Trim();
        if (!string.IsNullOrEmpty(tag) && !_tags.Contains(tag))
        {
            _tags.Add(tag);
            RenderTags();
            _tagInputTextBox.Clear();
        }
    }

    private void TagComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_tagComboBox.SelectedIndex > 0)
        {
            var selectedItem = _tagComboBox.SelectedItem as dynamic;
            var tag = selectedItem?.Tag?.ToString();
            if (!string.IsNullOrEmpty(tag) && !_tags.Contains(tag))
            {
                _tags.Add(tag);
                RenderTags();
            }
            _tagComboBox.SelectedIndex = 0;
        }
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "图片文件|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.ico|所有文件|*.*",
            Title = "选择Logo"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _tempLogoPath = dialog.FileName;
            LoadLogoPreview(dialog.FileName);
        }
    }

    private void LoadLogoPreview(string path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path))
            {
                var fullPath = path;
                if (!Path.IsPathRooted(path))
                {
                    fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                }

                if (File.Exists(fullPath))
                {
                    using var original = Image.FromFile(fullPath);
                    _logoPreview.Image = new Bitmap(original);
                    return;
                }
            }
        }
        catch { }

        _logoPreview.Image = null;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        var name = _nameTextBox.Text.Trim();
        var url = _urlTextBox.Text.Trim();
        var desc = _descTextBox.Text.Trim();

        if (string.IsNullOrEmpty(name))
        {
            MessageBox.Show("请输入网站名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameTextBox.Focus();
            return;
        }

        if (string.IsNullOrEmpty(url))
        {
            MessageBox.Show("请输入网站地址", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _urlTextBox.Focus();
            return;
        }

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
            url = "https://" + url;
        }

        Website.Name = name;
        Website.Url = url;
        Website.Description = desc;
        Website.Tags = [.. _tags];
        Website.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrEmpty(_tempLogoPath))
        {
            var newLogoPath = ConfigService.CopyLogo(_tempLogoPath, name);
            if (!string.IsNullOrEmpty(newLogoPath))
            {
                if (!string.IsNullOrEmpty(Website.LogoPath) && Website.LogoPath != newLogoPath)
                {
                    ConfigService.DeleteLogo(Website.LogoPath);
                }
                Website.LogoPath = newLogoPath;
            }
        }

        this.DialogResult = DialogResult.OK;
    }
}
