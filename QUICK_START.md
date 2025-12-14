# Quick Reference - Safety Toggle Commands

## ✅ Amuse with Safety DISABLED (Faster, Less Memory)

```powershell
# Option 1: CLI Flag (Recommended)
& "C:\Program Files\Amuse\Amuse.exe" --disable-safety

# Option 2: Environment Variable
$env:AMUSE_DISABLE_SAFETY = "1"
& "C:\Program Files\Amuse\Amuse.exe"

# Option 3: Config File
# Edit: C:\Program Files\Amuse\appsettings.json
# Change: "EnableSafetyChecker": false
& "C:\Program Files\Amuse\Amuse.exe"
```

**Expected Result**:
- Memory: **8-30 MB** ✓
- ContentFilter: **NOT loaded** ✓
- NSFW filtering: **DISABLED** ✓

---

## 🛡️ Amuse with Safety ENABLED (Original, Filters Content)

```powershell
# Just run normally (default)
& "C:\Program Files\Amuse\Amuse.exe"

# Or explicitly set EnableSafetyChecker to true in appsettings.json
```

**Expected Result**:
- Memory: **170+ MB** ✓
- ContentFilter: **LOADED** ✓
- NSFW filtering: **ENABLED** ✓

---

## 📊 Performance Comparison

| Metric | Safety Disabled | Safety Enabled |
|--------|-----------------|----------------|
| Memory | 8-30 MB ✓ | 170+ MB |
| Startup Time | Faster | Slower |
| Generation Speed | 5-10% faster | Normal |
| NSFW Filtering | ✗ Disabled | ✓ Enabled |

---

## 🔧 Troubleshooting

### Check if Safety is Disabled
```powershell
# Should be ~8-30 MB if disabled
Get-Process Amuse | Select ProcessName, @{L='Memory(MB)';E={[math]::Round($_.WorkingSet64/1MB,1)}}
```

### Force Enable Safety (Reset to Default)
```powershell
# Unset environment variable
Remove-Item env:AMUSE_DISABLE_SAFETY -ErrorAction SilentlyContinue

# Restart Amuse
& "C:\Program Files\Amuse\Amuse.exe"
```

### View Logs
```powershell
Get-ChildItem "$env:APPDATA\Amuse\Logs" -Filter "*.log" | Sort LastWriteTime -Descending | Select -First 1 | Get-Content | Select -Last 50
```

---

## 📝 Git Status

**Repository**: https://github.com/hendrybui/AmuseAI  
**Branch**: `AmuseA1+`  
**Latest Commit**: See deployment success  

**Pull Latest Code**:
```powershell
cd C:\Users\kentb\repo\AmuseAI
git pull origin AmuseA1+
```

---

**✅ Safety toggle is ready to use!**
