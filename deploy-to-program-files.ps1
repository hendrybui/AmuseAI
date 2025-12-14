# Deploy modified Amuse to Program Files
# MUST RUN AS ADMINISTRATOR

$buildPath = "C:\Users\kentb\repo\AmuseAI\Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0"
$installPath = "C:\Program Files\Amuse"

Write-Host "Deploying Amuse with Safety Toggle to Program Files..." -ForegroundColor Cyan

# Check admin rights
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Right-click PowerShell and select 'Run as Administrator', then run this script again." -ForegroundColor Yellow
    exit 1
}

# Stop any running Amuse processes
Write-Host "Stopping Amuse processes..." -ForegroundColor Yellow
Get-Process Amuse* -ErrorAction SilentlyContinue | Stop-Process -Force

# Backup existing files
$backupPath = "$installPath\Backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Write-Host "Creating backup at: $backupPath" -ForegroundColor Yellow
New-Item -Path $backupPath -ItemType Directory -Force | Out-Null
Copy-Item "$installPath\Amuse.dll" -Destination $backupPath -Force -ErrorAction SilentlyContinue
Copy-Item "$installPath\Amuse.exe" -Destination $backupPath -Force -ErrorAction SilentlyContinue
Copy-Item "$installPath\Amuse.pdb" -Destination $backupPath -Force -ErrorAction SilentlyContinue

# Deploy new binaries
Write-Host "Deploying modified binaries..." -ForegroundColor Green
Copy-Item "$buildPath\Amuse.dll" -Destination $installPath -Force
Copy-Item "$buildPath\Amuse.exe" -Destination $installPath -Force
Copy-Item "$buildPath\Amuse.pdb" -Destination $installPath -Force
Copy-Item "$buildPath\Amuse.runtimeconfig.json" -Destination $installPath -Force
Copy-Item "$buildPath\Amuse.deps.json" -Destination $installPath -Force
Copy-Item "$buildPath\appdefaults.json" -Destination $installPath -Force -ErrorAction SilentlyContinue

# Deploy ONNX Runtime managed assemblies (required for our build)
Write-Host "Deploying ONNX Runtime assemblies..." -ForegroundColor Green
Copy-Item "$buildPath\Microsoft.ML.OnnxRuntime.dll" -Destination $installPath -Force -ErrorAction SilentlyContinue
Copy-Item "$buildPath\Microsoft.ML.OnnxRuntime.Extensions.dll" -Destination $installPath -Force -ErrorAction SilentlyContinue
Copy-Item "$buildPath\Microsoft.ML.Tokenizers.dll" -Destination $installPath -Force -ErrorAction SilentlyContinue

# Deploy Microsoft.Extensions assemblies (required for dependency injection)
Write-Host "Deploying Microsoft.Extensions assemblies..." -ForegroundColor Green
Get-ChildItem "$buildPath\Microsoft.Extensions.*.dll" | ForEach-Object {
    Copy-Item $_.FullName -Destination $installPath -Force -ErrorAction SilentlyContinue
}

# Deploy OnnxStack assemblies
Write-Host "Deploying OnnxStack assemblies..." -ForegroundColor Green
Get-ChildItem "$buildPath\OnnxStack.*.dll" | ForEach-Object {
    Copy-Item $_.FullName -Destination $installPath -Force -ErrorAction SilentlyContinue
}

# Deploy other required assemblies
Write-Host "Deploying other dependencies..." -ForegroundColor Green
$otherDlls = @("Serilog*.dll", "System.*.dll", "Google.Protobuf.dll", "ColorPicker.dll", "Microsoft.Xaml.Behaviors.dll")
foreach ($pattern in $otherDlls) {
    Get-ChildItem "$buildPath\$pattern" -ErrorAction SilentlyContinue | ForEach-Object {
        Copy-Item $_.FullName -Destination $installPath -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "`nDeployment complete!" -ForegroundColor Green
Write-Host "`nYou can now run: " -NoNewline
Write-Host "Amuse.exe --disable-safety" -ForegroundColor Cyan
Write-Host "Or set environment variable: " -NoNewline
Write-Host "`$env:AMUSE_DISABLE_SAFETY=1" -ForegroundColor Cyan
Write-Host "`nBackup location: $backupPath" -ForegroundColor Yellow
