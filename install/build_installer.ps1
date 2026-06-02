# sky Web Navigator Installer Build Script
# Usage: Run this script to build the installer

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "Building sky Web Navigator Installer" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build and publish the project
Write-Host 'Step 1: Building and publishing the project...' -ForegroundColor Yellow

$projectPath = Join-Path $PSScriptRoot '..'
$publishPath = Join-Path $projectPath 'publish'

# Clean previous build
Write-Host 'Cleaning previous build...'
dotnet clean $projectPath --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host 'ERROR: Clean failed!' -ForegroundColor Red
    Read-Host 'Press Enter to exit'
    exit 1
}

# Build and publish
Write-Host 'Publishing project...'
dotnet publish (Join-Path $projectPath 'WebNavigator.csproj') --configuration Release --output $publishPath

if ($LASTEXITCODE -ne 0) {
    Write-Host 'ERROR: Publish failed!' -ForegroundColor Red
    Read-Host 'Press Enter to exit'
    exit 1
}

Write-Host 'Project published successfully!' -ForegroundColor Green
Write-Host ''

# Step 2: Check if Inno Setup is installed
Write-Host 'Step 2: Checking for Inno Setup...' -ForegroundColor Yellow

$ISCC_PATH = $null
$possiblePaths = @(
    'C:\Program Files (x86)\Inno Setup 7\ISCC.exe',
    'C:\Program Files\Inno Setup 7\ISCC.exe',
    'C:\Program Files (x86)\Inno Setup 6\ISCC.exe',
    'C:\Program Files\Inno Setup 6\ISCC.exe'
)

foreach ($path in $possiblePaths) {
    if (Test-Path $path) {
        $ISCC_PATH = $path
        break
    }
}

if (-not $ISCC_PATH) {
    Write-Host 'ERROR: Inno Setup compiler not found!' -ForegroundColor Red
    Write-Host 'Please install Inno Setup from: https://jrsoftware.org/isdl.php' -ForegroundColor Yellow
    Read-Host 'Press Enter to exit'
    exit 1
}

Write-Host "Found Inno Setup: $ISCC_PATH" -ForegroundColor Green
Write-Host ''

# Step 3: Build installer
Write-Host 'Step 3: Compiling installer...' -ForegroundColor Yellow

& $ISCC_PATH (Join-Path $PSScriptRoot 'install.iss')

if ($LASTEXITCODE -eq 0) {
    $outputDir = Join-Path $PSScriptRoot 'output'
    Write-Host ''
    Write-Host 'Installer built successfully!' -ForegroundColor Green
    Write-Host "Output: $outputDir\skyの自定义导航Setup.exe" -ForegroundColor Green
} else {
    Write-Host ''
    Write-Host 'Installer build failed!' -ForegroundColor Red
}

Read-Host 'Press Enter to exit'