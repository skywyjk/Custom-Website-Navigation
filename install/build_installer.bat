@echo off
echo ==============================================
echo 构建 skyの自定义导航 安装程序
echo ==============================================
echo.

:: 检查 Inno Setup 是否安装
set "ISCC_PATH="
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" set "ISCC_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if exist "C:\Program Files\Inno Setup 6\ISCC.exe" set "ISCC_PATH=C:\Program Files\Inno Setup 6\ISCC.exe"

if not defined ISCC_PATH (
    echo 错误: 未找到 Inno Setup 编译器
    echo 请先安装 Inno Setup 6: https://jrsoftware.org/isdl.php
    pause
    exit /b 1
)

:: 创建输出目录
mkdir ..\output 2>nul

:: 编译安装程序
echo 正在编译安装程序...
"%ISCC_PATH%" install.iss

if %errorlevel% equ 0 (
    echo.
    echo 安装程序构建成功！
    echo 输出文件: ..\output\WebNavigatorSetup.exe
) else (
    echo.
    echo 安装程序构建失败！
)

pause