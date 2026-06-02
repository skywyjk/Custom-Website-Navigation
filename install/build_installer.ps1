# skyの自定义导航 安装程序构建脚本

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "构建 skyの自定义导航 安装程序" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

# 检查 Inno Setup 是否安装
$ISCC_PATH = $null
$possiblePaths = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
)

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $ISCC_PATH = $path
        break
    }
}

if (-not $ISCC_PATH) {
    Write-Host "错误: 未找到 Inno Setup 编译器" -ForegroundColor Red
    Write-Host "请先安装 Inno Setup 6: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Read-Host "按 Enter 键退出"
    exit 1
}

Write-Host "找到 Inno Setup 编译器: $ISCC_PATH" -ForegroundColor Green

# 创建输出目录
$outputDir = Join-Path $PSScriptRoot "..\output"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# 编译安装程序
Write-Host ""
Write-Host "正在编译安装程序..." -ForegroundColor Yellow

& $ISCC_PATH (Join-Path $PSScriptRoot "install.iss")

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "安装程序构建成功！" -ForegroundColor Green
    Write-Host "输出文件: $outputDir\WebNavigatorSetup.exe" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "安装程序构建失败！" -ForegroundColor Red
}

Read-Host "按 Enter 键退出"