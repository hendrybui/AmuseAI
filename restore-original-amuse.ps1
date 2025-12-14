# Restore Original Amuse from Latest Backup

$backupDir = Get-ChildItem "C:\Program Files\Amuse\Backup_*" -Directory | Sort-Object CreationTime -Descending | Select-Object -First 1

if ($backupDir) {
    Write-Host "Restoring from: $($backupDir.Name)" -ForegroundColor Cyan
    Copy-Item "$($backupDir.FullName)\*" -Destination "C:\Program Files\Amuse" -Force -Recurse
    Write-Host "Restored original Amuse!" -ForegroundColor Green
    
    # Test
    Start-Sleep -Seconds 2
    Start-Process "C:\Program Files\Amuse\Amuse.exe"
    Start-Sleep -Seconds 10
    
    $proc = Get-Process Amuse -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "`nOriginal Amuse is running!" -ForegroundColor Green
    } else {
        Write-Host "`nOriginal Amuse still not running" -ForegroundColor Red
    }
} else {
    Write-Host "No backup found!" -ForegroundColor Red
}
