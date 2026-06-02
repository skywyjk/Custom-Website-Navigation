# skyの自定义导航

一个简洁美观的网站导航桌面应用程序，使用 C# WinForms 开发。

## 功能特性

- 🎨 美观的 UI 界面
- 🏷️ 标签管理 - 支持为网站添加标签分类
- 🔍 标签筛选 - 可按标签快速筛选网站
- 📝 配置文件使用 JSON 格式，易于编辑
- 🖼️ 网站 Logo 自动管理，自动复制到 logos 文件夹
- ➕ 支持添加、编辑、删除网站
- 🔗 点击「前往」按钮直接打开对应网站
- 🔑 管理员权限 - 程序默认以管理员权限运行
- 🖥️ 窗口置顶 - 安装/卸载窗口始终置顶显示
- 🗑️ 完整卸载 - 卸载时彻底删除所有相关文件
- 🚀 自动启动 - 安装完成后自动启动应用

## 项目结构

```
webdh/
├── WebNavigator.csproj    # 项目文件
├── WebNavigator.sln       # 解决方案文件
├── logo.ico               # 程序图标
├── Program.cs             # 程序入口
├── config.json            # 默认配置文件
├── LICENSE.txt            # 许可证文件
├── Models/
│   ├── WebsiteItem.cs     # 网站数据模型
│   └── AppConfig.cs       # 配置数据模型
├── Services/
│   └── ConfigService.cs   # 配置管理服务
├── Forms/
│   ├── MainForm.cs        # 主窗体
│   ├── EditForm.cs        # 添加/编辑网站窗体
│   └── TagManagerForm.cs  # 标签管理窗体
├── Utils/
│   └── ResourceHelper.cs  # 资源辅助工具
├── install/               # 安装程序
│   ├── install.iss        # Inno Setup 脚本
│   ├── build_installer.ps1 # PowerShell 构建脚本
│   ├── build_installer.bat # 批处理构建脚本
│   ├── languages/         # 语言文件目录
│   │   └── ChineseSimplified.isl
│   └── output/            # 安装包输出目录
└── publish/               # 发布输出目录
```

## 如何编译运行

### 前置要求

- .NET 8.0 SDK 或更高版本

### 编译与运行

在项目目录下执行：

```bash
dotnet build
dotnet run
```

## 发布为可执行文件

```bash
dotnet publish -c Release -o publish
```

发布后的文件在 `publish` 文件夹中。

## 构建安装程序

### 前置要求

- 安装 [Inno Setup 7](https://jrsoftware.org/isdl.php)（推荐使用最新版本）

### 构建步骤

```bash
cd install
build_installer.bat
```

安装程序会输出到 `install/output/文件名`

### 安装程序特性

- ✅ **管理员权限**：安装程序需要管理员权限运行
- ✅ **默认创建桌面快捷方式**：安装时自动创建桌面快捷方式
- ✅ **程序以管理员权限运行**：WebNavigator.exe 默认以管理员权限启动
- ✅ **卸载程序以管理员权限运行**：卸载程序默认以管理员权限启动
- ✅ **窗口置顶**：安装和卸载窗口始终置顶显示
- ✅ **完整卸载**：卸载时彻底删除安装目录及其所有内容（包括 config 和 logo 文件夹）
- ✅ **安装后自动启动**：安装完成后自动启动应用程序

## 配置文件说明

配置文件保存在程序目录下的 `config/config.json`，格式如下：

```json
{
  "version": "1.0.0",
  "websites": [
    {
      "id": "唯一标识符",
      "name": "网站名称",
      "url": "网站地址",
      "logoPath": "logos/文件名.png",
      "description": "描述",
      "tags": ["标签1", "标签2"],
      "createdAt": "创建时间",
      "updatedAt": "更新时间"
    }
  ],
  "tags": ["已定义的标签"],
  "lastModified": "最后修改时间"
}
```

##

