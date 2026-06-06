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
$signPath = Join-Path $PSScriptRoot 'sign'

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

# Clean up .pdb files before packaging
Write-Host 'Cleaning up .pdb files...' -ForegroundColor Yellow
$pdbFiles = Get-ChildItem -Path $publishPath -Filter "*.pdb" -ErrorAction SilentlyContinue
if ($pdbFiles) {
    foreach ($pdbFile in $pdbFiles) {
        Write-Host "  Deleting: $($pdbFile.Name)" -ForegroundColor Gray
        Remove-Item -Path $pdbFile.FullName -Force
    }
    Write-Host "  PDB files cleaned up" -ForegroundColor Green
}
Write-Host ''

# Step 2: Sign published files
Write-Host 'Step 2: Signing published files...' -ForegroundColor Yellow

# Find PFX file in sign directory
$pfxFile = Get-ChildItem -Path $signPath -Filter "*.pfx" -ErrorAction SilentlyContinue | Select-Object -First 1

if ($pfxFile) {
    Write-Host "Found PFX file: $($pfxFile.FullName)" -ForegroundColor Green
    
    # Copy cer file to publish directory
    $cerFile = Get-ChildItem -Path $signPath -Filter "*.cer" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($cerFile) {
        Copy-Item -Path $cerFile.FullName -Destination (Join-Path $publishPath $cerFile.Name)
        Write-Host "Copied CER file: $($cerFile.Name)" -ForegroundColor Green
    }
    
    # Get certificate password
    $pfxPassword = Read-Host "Enter PFX password (press Enter if no password)" -AsSecureString
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($pfxPassword))
    
    # Find signtool.exe - more comprehensive search
    Write-Host "Searching for signtool.exe..." -ForegroundColor Yellow
    
    $signtoolPath = $null
    $signtoolPossiblePaths = @()
    
    # Add Windows 10/11 SDK paths
    for ($i = 22; $i -ge 10; $i--) {
        $signtoolPossiblePaths += "C:\Program Files (x86)\Windows Kits\10\bin\10.0.$i.0\x64\signtool.exe"
        $signtoolPossiblePaths += "C:\Program Files (x86)\Windows Kits\10\bin\10.0.$i.0\x86\signtool.exe"
        $signtoolPossiblePaths += "C:\Program Files\Windows Kits\10\bin\10.0.$i.0\x64\signtool.exe"
        $signtoolPossiblePaths += "C:\Program Files\Windows Kits\10\bin\10.0.$i.0\x86\signtool.exe"
    }
    
    # Add older Windows Kits
    $signtoolPossiblePaths += "C:\Program Files (x86)\Windows Kits\8.1\bin\x64\signtool.exe"
    $signtoolPossiblePaths += "C:\Program Files (x86)\Windows Kits\8.1\bin\x86\signtool.exe"
    $signtoolPossiblePaths += "C:\Program Files (x86)\Windows Kits\8.0\bin\x64\signtool.exe"
    $signtoolPossiblePaths += "C:\Program Files (x86)\Windows Kits\8.0\bin\x86\signtool.exe"
    $signtoolPossiblePaths += "${env:ProgramFiles(x86)}\Microsoft SDKs\Windows\v7.1A\Bin\signtool.exe"
    
    # Check each path
    foreach ($path in $signtoolPossiblePaths) {
        if (Test-Path $path) {
            $signtoolPath = $path
            Write-Host "Found signtool at: $signtoolPath" -ForegroundColor Green
            break
        }
    }
    
    # Also try to find via PATH
    if (-not $signtoolPath) {
        $whereResult = where.exe signtool 2>$null
        if ($whereResult) {
            $signtoolPath = $whereResult | Select-Object -First 1
            Write-Host "Found signtool in PATH: $signtoolPath" -ForegroundColor Green
        }
    }
    
    if (-not $signtoolPath) {
        Write-Host "ERROR: signtool.exe not found!" -ForegroundColor Red
        Write-Host "Please install Windows SDK from: https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/" -ForegroundColor Yellow
        Read-Host 'Press Enter to exit'
        exit 1
    }
    
    # Sign all executable and dll files
    Write-Host ""
    $filesToSign = @()
    $filesToSign += Get-ChildItem -Path $publishPath -Filter "*.exe"
    $filesToSign += Get-ChildItem -Path $publishPath -Filter "*.dll"
    
    foreach ($file in $filesToSign) {
        Write-Host "Signing: $($file.Name)" -ForegroundColor Cyan
        
        # SHA1 signing with Authenticode timestamp (old protocol)
        Write-Host "  Signing with SHA1..." -ForegroundColor Gray
        $sha1Success = $false
        
        $signArgs = @(
            "sign",
            "/f", $pfxFile.FullName
        )
        if (-not [string]::IsNullOrEmpty($plainPassword)) {
            $signArgs += "/p", $plainPassword
        }
        $signArgs += "/fd", "sha1"
        $signArgs += "/t", "http://timestamp.digicert.com"
        $signArgs += $file.FullName
        
        $sha1Output = & $signtoolPath @signArgs 2>&1
        if ($LASTEXITCODE -eq 0) {
            $sha1Success = $true
            Write-Host "    SHA1 signed successfully" -ForegroundColor Gray
        } else {
            Write-Host "    SHA1 signing output: $sha1Output" -ForegroundColor DarkGray
        }
        
        # SHA256 signing with RFC 3161 timestamp (new protocol)
        Write-Host "  Signing with SHA256..." -ForegroundColor Gray
        $signArgs = @(
            "sign",
            "/f", $pfxFile.FullName
        )
        if (-not [string]::IsNullOrEmpty($plainPassword)) {
            $signArgs += "/p", $plainPassword
        }
        $signArgs += "/fd", "sha256"
        $signArgs += "/tr", "http://timestamp.digicert.com"
        $signArgs += "/td", "sha256"
        
        if ($sha1Success) {
            $signArgs += "/as"
        }
        
        $signArgs += $file.FullName
        
        $sha256Output = & $signtoolPath @signArgs 2>&1
        $sha256Success = ($LASTEXITCODE -eq 0)
        
        if ($sha256Success) {
            if ($sha1Success) {
                Write-Host "  Successfully signed with SHA1 + SHA256" -ForegroundColor Green
            } else {
                Write-Host "  Successfully signed with SHA256 only" -ForegroundColor Yellow
                Write-Host "    SHA256 output: $sha256Output" -ForegroundColor DarkGray
            }
        } else {
            Write-Host "  Signing failed" -ForegroundColor Red
            Write-Host "    Output: $sha256Output" -ForegroundColor DarkGray
        }
    }
} else {
    Write-Host "WARNING: No PFX file found in sign directory, skipping signing" -ForegroundColor Yellow
    Write-Host "Sign directory: $signPath" -ForegroundColor Gray
    Write-Host "Place your PFX and CER files in this directory to enable signing" -ForegroundColor Gray
}

Write-Host ''

# Step 3: Check if Inno Setup is installed
Write-Host 'Step 3: Checking for Inno Setup...' -ForegroundColor Yellow

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

# Step 4: Build installer
Write-Host 'Step 4: Compiling installer...' -ForegroundColor Yellow
$outputDir = Join-Path $PSScriptRoot 'output'

& $ISCC_PATH (Join-Path $PSScriptRoot 'install.iss')

if ($LASTEXITCODE -eq 0) {
    Write-Host ''
    Write-Host 'Installer built successfully!' -ForegroundColor Green
    Write-Host "Output: $outputDir" -ForegroundColor Green
    
    # Sign the installer if PFX is available
    if ($pfxFile -and $signtoolPath) {
        Write-Host ''
        Write-Host 'Step 5: Signing the installer...' -ForegroundColor Yellow
        
        $installerFile = Get-ChildItem -Path $outputDir -Filter "*.exe" | Select-Object -First 1
        if ($installerFile) {
            Write-Host "Signing installer: $($installerFile.Name)" -ForegroundColor Cyan
            
            # SHA1 signing with Authenticode timestamp (old protocol)
            Write-Host "  Signing with SHA1..." -ForegroundColor Gray
            $sha1Success = $false
            
            $signArgs = @(
                "sign",
                "/f", $pfxFile.FullName
            )
            if (-not [string]::IsNullOrEmpty($plainPassword)) {
                $signArgs += "/p", $plainPassword
            }
            $signArgs += "/fd", "sha1"
            $signArgs += "/t", "http://timestamp.digicert.com"
            $signArgs += $installerFile.FullName
            
            $sha1Output = & $signtoolPath @signArgs 2>&1
            if ($LASTEXITCODE -eq 0) {
                $sha1Success = $true
                Write-Host "    SHA1 signed successfully" -ForegroundColor Gray
            } else {
                Write-Host "    SHA1 signing output: $sha1Output" -ForegroundColor DarkGray
            }
            
            # SHA256 signing with RFC 3161 timestamp (new protocol)
            Write-Host "  Signing with SHA256..." -ForegroundColor Gray
            $signArgs = @(
                "sign",
                "/f", $pfxFile.FullName
            )
            if (-not [string]::IsNullOrEmpty($plainPassword)) {
                $signArgs += "/p", $plainPassword
            }
            $signArgs += "/fd", "sha256"
            $signArgs += "/tr", "http://timestamp.digicert.com"
            $signArgs += "/td", "sha256"
            
            if ($sha1Success) {
                $signArgs += "/as"
            }
            
            $signArgs += $installerFile.FullName
            
            $sha256Output = & $signtoolPath @signArgs 2>&1
            $sha256Success = ($LASTEXITCODE -eq 0)
            
            if ($sha256Success) {
                if ($sha1Success) {
                    Write-Host "  Installer signed with SHA1 + SHA256!" -ForegroundColor Green
                } else {
                    Write-Host "  Installer signed with SHA256 only" -ForegroundColor Yellow
                    Write-Host "    SHA256 output: $sha256Output" -ForegroundColor DarkGray
                }
            } else {
                Write-Host "  Installer signing failed" -ForegroundColor Red
                Write-Host "    Output: $sha256Output" -ForegroundColor DarkGray
            }
        }
    }
} else {
    Write-Host ''
    Write-Host 'Installer build failed!' -ForegroundColor Red
}

Read-Host 'Press Enter to exit'