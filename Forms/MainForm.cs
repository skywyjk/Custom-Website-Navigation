using System.Diagnostics;
using System.Drawing.Drawing2D;
using WebNavigator.Models;
using WebNavigator.Services;

namespace WebNavigator.Forms;

public partial class MainForm : Form
{
    private AppConfig _config = new();
    private FlowLayoutPanel _websitePanel = null!;
    private FlowLayoutPanel _tagFilterPanel = null!;
    private Button _addButton = null!;
    private string? _selectedTag = null;

    public MainForm()
    {
        InitializeComponent();
        LoadConfig();
    }

    private void InitializeComponent()
    {
        this.Text = "skyの自定义导航";
        this.Size = new Size(1400, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.MinimumSize = new Size(800, 600);
        this.BackColor = Color.FromArgb(240, 242, 245);

        var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.ico");
        if (File.Exists(iconPath))
        {
            this.Icon = new Icon(iconPath);
        }

        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.White,
            Padding = new Padding(20, 0, 20, 0)
        };

        _tagFilterPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.FromArgb(250, 250, 252),
            Padding = new Padding(20, 5, 20, 20),
            AutoScroll = true
        };

        var titleLabel = new Label
        {
            Text = "  skyの自定义导航",
            Font = new Font("Microsoft YaHei UI", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 51, 51),
            Location = new Point(0, 20),
            AutoSize = true
        };

        _addButton = new Button
        {
            Text = "➕ 添加网站",
            Font = new Font("Microsoft YaHei UI", 10F),
            Size = new Size(140, 40),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _addButton.FlatAppearance.BorderSize = 0;
        _addButton.Click += AddButton_Click;

        var tagManagerButton = new Button
        {
            Text = "🏷️ 标签管理",
            Font = new Font("Microsoft YaHei UI", 10F),
            Size = new Size(140, 40),
            BackColor = Color.FromArgb(255, 152, 0),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        tagManagerButton.FlatAppearance.BorderSize = 0;
        tagManagerButton.Click += TagManagerButton_Click;

        topPanel.Controls.Add(titleLabel);
        topPanel.Controls.Add(tagManagerButton);
        topPanel.Controls.Add(_addButton);

        _addButton.Location = new Point(topPanel.Width - _addButton.Width - 20, 15);
        tagManagerButton.Location = new Point(_addButton.Left - tagManagerButton.Width - 10, 15);
        topPanel.Resize += (s, e) =>
        {
            _addButton.Location = new Point(topPanel.Width - _addButton.Width - 20, 15);
            tagManagerButton.Location = new Point(_addButton.Left - tagManagerButton.Width - 10, 15);
        };

        _websitePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 60, 20, 20),
            AutoScroll = true,
            BackColor = Color.FromArgb(240, 242, 245)
        };

        this.Controls.Add(_tagFilterPanel);
        this.Controls.Add(_websitePanel);
        this.Controls.Add(topPanel);
    }

    private void LoadConfig()
    {
        _config = ConfigService.LoadConfig();
        RenderTagFilters();
        RenderWebsites();
    }

    private void RenderTagFilters()
    {
        _tagFilterPanel.Controls.Clear();

        var allButton = CreateTagFilterButton("全部", _selectedTag == null);
        allButton.Click += (s, e) =>
        {
            _selectedTag = null;
            RenderTagFilters();
            RenderWebsites();
        };
        _tagFilterPanel.Controls.Add(allButton);

        var allTags = _config.Websites.SelectMany(w => w.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t);

        foreach (var tag in allTags)
        {
            var tagButton = CreateTagFilterButton(tag, _selectedTag == tag);
            var tagCopy = tag;
            tagButton.Click += (s, e) =>
            {
                _selectedTag = tagCopy;
                RenderTagFilters();
                RenderWebsites();
            };
            _tagFilterPanel.Controls.Add(tagButton);
        }
    }

    private static Button CreateTagFilterButton(string text, bool isSelected)
    {
        var button = new Button
        {
            Text = text,
            Size = new Size(0, 30),
            AutoSize = true,
            Padding = new Padding(12, 5, 12, 5),
            BackColor = isSelected ? Color.FromArgb(24, 144, 255) : Color.White,
            ForeColor = isSelected ? Color.White : Color.FromArgb(51, 51, 51),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F),
            Margin = new Padding(3)
        };
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = isSelected ? Color.FromArgb(24, 144, 255) : Color.FromArgb(220, 220, 220);
        return button;
    }

    private void RenderWebsites()
    {
        _websitePanel.Controls.Clear();

        IEnumerable<WebsiteItem> filteredWebsites = string.IsNullOrEmpty(_selectedTag)
            ? _config.Websites
            : _config.Websites.Where(w => w.Tags.Contains(_selectedTag));

        foreach (var website in filteredWebsites)
        {
            var card = CreateWebsiteCard(website);
            _websitePanel.Controls.Add(card);
        }
    }

    private Panel CreateWebsiteCard(WebsiteItem website)
    {
        var card = new Panel
        {
            Size = new Size(240, 380),
            Margin = new Padding(10),
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };

        card.Paint += (s, e) =>
        {
            if (s is not Panel panel) return;
            using var path = new GraphicsPath();
            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            path.AddArc(rect.X, rect.Y, 10, 10, 180, 90);
            path.AddArc(rect.Right - 10, rect.Y, 10, 10, 270, 90);
            path.AddArc(rect.Right - 10, rect.Bottom - 10, 10, 10, 0, 90);
            path.AddArc(rect.X, rect.Bottom - 10, 10, 10, 90, 90);
            path.CloseAllFigures();
            panel.Region = new Region(path);
        };

        var logoBox = new PictureBox
        {
            Size = new Size(100, 100),
            Location = new Point(70, 30),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        try
        {
            var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, website.LogoPath);
            if (File.Exists(logoPath))
            {
                using var original = Image.FromFile(logoPath);
                logoBox.Image = new Bitmap(original);
            }
            else
            {
                logoBox.Image = CreateDefaultLogo(website.Name);
            }
        }
        catch
        {
            logoBox.Image = CreateDefaultLogo(website.Name);
        }

        var nameLabel = new Label
        {
            Text = website.Name,
            Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 51, 51),
            Location = new Point(30, 150),
            Size = new Size(180, 25),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var descLabel = new Label
        {
            Text = website.Description.Length > 30 ? string.Concat(website.Description.AsSpan(0, 30), "...") : website.Description,
            Font = new Font("Microsoft YaHei UI", 8.5F),
            ForeColor = Color.FromArgb(153, 153, 153),
            Location = new Point(30, 180),
            Size = new Size(180, 40),
            TextAlign = ContentAlignment.TopCenter
        };

        var tagPanel = CreateTagDisplayPanel(website.Tags, 30, 225);

        var goButton = new Button
        {
            Text = "前往",
            Size = new Size(180, 40),
            Location = new Point(30, 280),
            BackColor = Color.FromArgb(24, 144, 255),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold)
        };
        goButton.FlatAppearance.BorderSize = 0;
        goButton.Click += (s, e) => OpenWebsite(website);

        var editButton = new Button
        {
            Text = "编辑",
            Size = new Size(85, 30),
            Location = new Point(30, 330),
            BackColor = Color.FromArgb(248, 249, 250),
            ForeColor = Color.FromArgb(102, 102, 102),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        editButton.FlatAppearance.BorderSize = 0;
        editButton.Click += (s, e) => EditWebsite(website);

        var deleteButton = new Button
        {
            Text = "删除",
            Size = new Size(85, 30),
            Location = new Point(125, 330),
            BackColor = Color.FromArgb(255, 77, 79),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        deleteButton.FlatAppearance.BorderSize = 0;
        deleteButton.Click += (s, e) => DeleteWebsite(website);

        card.Click += (s, e) => OpenWebsite(website);
        logoBox.Click += (s, e) => OpenWebsite(website);
        nameLabel.Click += (s, e) => OpenWebsite(website);
        descLabel.Click += (s, e) => OpenWebsite(website);

        card.Controls.Add(logoBox);
        card.Controls.Add(nameLabel);
        card.Controls.Add(descLabel);
        card.Controls.Add(tagPanel);
        card.Controls.Add(goButton);
        card.Controls.Add(editButton);
        card.Controls.Add(deleteButton);

        return card;
    }

    private static Panel CreateTagDisplayPanel(List<string> tags, int x, int y)
    {
        var tagPanel = new Panel
        {
            Location = new Point(x, y),
            Size = new Size(180, 60),
            BackColor = Color.Transparent,
            AutoSize = false
        };

        var currentY = 0;
        int currentX;
        var maxWidth = 180;
        var tagHeight = 22;
        var spacing = 4;

        var tagItems = new List<Label>();
        foreach (var tag in tags.Take(6))
        {
            var tagFont = new Font("Microsoft YaHei UI", 8F);
            var textWidth = TextRenderer.MeasureText(tag, tagFont).Width;
            var tagLabel = new Label
            {
                Text = tag,
                AutoSize = false,
                Size = new Size(textWidth + 16, tagHeight),
                BackColor = Color.FromArgb(24, 144, 255),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = tagFont
            };
            tagItems.Add(tagLabel);
        }

        if (tags.Count > 6)
        {
            var moreLabel = new Label
            {
                Text = $"+{tags.Count - 6}",
                AutoSize = false,
                Size = new Size(30, tagHeight),
                BackColor = Color.FromArgb(153, 153, 153),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Microsoft YaHei UI", 8F),
                Padding = new Padding(2)
            };
            tagItems.Add(moreLabel);
        }

        var rowStartIndex = 0;
        while (rowStartIndex < tagItems.Count)
        {
            var rowWidth = 0;
            var rowEndIndex = rowStartIndex;

            while (rowEndIndex < tagItems.Count)
            {
                var nextWidth = rowWidth + tagItems[rowEndIndex].Width + (rowEndIndex > rowStartIndex ? spacing : 0);
                if (nextWidth <= maxWidth)
                {
                    rowWidth = nextWidth;
                    rowEndIndex++;
                }
                else
                {
                    break;
                }
            }

            var startX = (maxWidth - rowWidth) / 2;
            currentX = startX;

            for (int i = rowStartIndex; i < rowEndIndex; i++)
            {
                tagItems[i].Location = new Point(currentX, currentY);
                tagPanel.Controls.Add(tagItems[i]);
                currentX += tagItems[i].Width + spacing;
            }

            currentY += tagHeight + spacing;
            rowStartIndex = rowEndIndex;
        }

        return tagPanel;
    }

    private static Bitmap CreateDefaultLogo(string name)
    {
        var bmp = new Bitmap(100, 100);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(24, 144, 255));
        using var font = new Font("Microsoft YaHei UI", 30F, FontStyle.Bold);
        using var brush = new SolidBrush(Color.White);
        var text = name.Length > 0 ? name[0].ToString().ToUpper() : "?";
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, (100 - size.Width) / 2, (100 - size.Height) / 2);
        return bmp;
    }

    private static void OpenWebsite(WebsiteItem website)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = website.Url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"无法打开网站: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        using var form = new EditForm(null);
        if (form.ShowDialog() == DialogResult.OK)
        {
            var newWebsite = form.Website;
            _config.Websites.Add(newWebsite);
            ConfigService.SaveConfig(_config);
            RenderTagFilters();
            RenderWebsites();
        }
    }

    private void TagManagerButton_Click(object? sender, EventArgs e)
    {
        using var form = new TagManagerForm(_config);
        form.ShowDialog();
        _config = ConfigService.LoadConfig();
        RenderTagFilters();
        RenderWebsites();
    }

    private void EditWebsite(WebsiteItem website)
    {
        var oldLogoPath = website.LogoPath;
        using var form = new EditForm(website);
        if (form.ShowDialog() == DialogResult.OK)
        {
            var updated = form.Website;
            var index = _config.Websites.FindIndex(w => w.Id == website.Id);
            if (index >= 0)
            {
                if (!string.IsNullOrEmpty(oldLogoPath) && oldLogoPath != updated.LogoPath)
                {
                    ConfigService.DeleteLogo(oldLogoPath);
                }
                _config.Websites[index] = updated;
                ConfigService.SaveConfig(_config);
                RenderTagFilters();
                RenderWebsites();
            }
        }
    }

    private void DeleteWebsite(WebsiteItem website)
    {
        var result = MessageBox.Show(
            $"确定要删除「{website.Name}」吗？",
            "确认删除",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            ConfigService.DeleteLogo(website.LogoPath);
            _config.Websites.RemoveAll(w => w.Id == website.Id);
            ConfigService.SaveConfig(_config);
            RenderWebsites();
        }
    }
}
