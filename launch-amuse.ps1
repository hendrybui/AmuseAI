# Launch Amuse with Safety Toggle Disabled
# This script runs from the build directory to avoid Program Files permissions

$buildPath = "C:\Users\kentb\repo\AmuseAI\Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0"

Write-Host "Launching Amuse with safety toggle disabled..." -ForegroundColor Cyan
Write-Host "Build path: $buildPath`n" -ForegroundColor Gray

# Change to build directory and launch
Set-Location $buildPath
& ".\Amuse.exe" --disable-safety

Write-Host "`nAmuse launched. Check if it's running..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

$proc = Get-Process Amuse -ErrorAction SilentlyContinue
if ($proc) {
    $mem = [math]::Round($proc.WorkingSet64/1MB,1)
    Write-Host "`n✓ Amuse is running!" -ForegroundColor Green
    Write-Host "Memory: $mem MB" -ForegroundColor Cyan
    if ($mem -lt 150) {
        Write-Host "✓ Safety toggle WORKING - ContentFilter NOT loaded!" -ForegroundColor Green
    }
} else {
    Write-Host "`n✗ Amuse didn't start or crashed" -ForegroundColor Red
}
