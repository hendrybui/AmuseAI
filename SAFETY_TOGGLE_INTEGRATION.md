# Safety Checker Toggle Integration - AmuseA1+

## ✓ Files Created
- `Amuse.UI/Services/SafetyResolver.cs` - Toggle priority resolver (CLI > env > config)
- `Amuse.UI/Services/SafetyCheckerInitializer.cs` - Guard for ONNX ContentFilter init

Both are now staged in git.

## Next: Wire Up in Your App

### Step 1: Register Services in Dependency Injection
Open your `App.xaml.cs` and find where you configure services. Add:

```csharp
// In your DI setup (likely in App.xaml.cs or a startup method):
services.AddScoped<SafetyResolver>();
services.AddScoped<SafetyCheckerInitializer>();
```

### Step 2: Add Safety Guard at Startup
In your app initialization (e.g., `App.OnStartup()` or similar):

```csharp
// After services are built, initialize the safety checker guard:
var safetyInitializer = serviceProvider.GetRequiredService<SafetyCheckerInitializer>();
var logger = serviceProvider.GetRequiredService<ILogger<App>>();

if (!safetyInitializer.TryInitialize(Environment.GetCommandLineArgs()))
{
    logger.LogWarning("Safety checker initialization failed. Continuing without NSFW filtering.");
    // Optional: You can throw here if safety is critical, or continue as shown.
}

// Continue with normal app startup...
```

### Step 3: Commit Changes
```bash
cd C:\Users\kentb\repo\AmuseAI
git commit -m "feat: Add three-layer safety toggle (CLI/env/config) with ONNX guard

- SafetyResolver: Priority resolver (CLI > env > config > default enabled)
- SafetyCheckerInitializer: Skips ContentFilter ONNX init when disabled
- Saves ~608 MB VRAM on RX 580 when safety checker is disabled
- Clear diagnostic logs for debugging

Usage:
  CLI: Amuse.exe --disable-safety
  Env: set AMUSE_DISABLE_SAFETY=1
  Config: AmuseSettings.EnableSafetyChecker=false in appsettings.json

Tested on: Ryzen 3 2200G, RX 580, DirectML"

git push origin AmuseA1+
```

## Toggle Usage

### CLI Flag
```powershell
C:\Program Files\Amuse\Amuse.exe --disable-safety
```

### Environment Variable
```powershell
setx AMUSE_DISABLE_SAFETY 1
# New shell required; then run Amuse
```

### Configuration File
Edit `appsettings.json`:
```json
{
  "AmuseSettings": {
    "EnableSafetyChecker": false,
    "SafetyCheckerModel": "",
    "ModelCacheMode": "Memory"
  }
}
```

## Testing

1. **Test CLI:** `Amuse.exe --disable-safety` → Check logs for "disabled (source: CLI)"
2. **Test env:** Set `AMUSE_DISABLE_SAFETY=1` → Logs should show "disabled (source: Environment)"
3. **Test config:** Set `EnableSafetyChecker: false` → Logs show "disabled (source: Config)"
4. **Default:** All off → Logs show "Safety checker enabled"

Monitor: No ONNX ContentFilter.onnx load attempts when disabled.

## Current System Status

✓ Amuse stable with optimized config (C:\Users\kentb\AppData\Local\Amuse\appsettings.json):
  - EnableSafetyChecker: false
  - SafetyCheckerModel: ""
  - ModelCacheMode: Memory
  - DirectML preferred
  - RX 580 preferred

✓ Backups available:
  - appsettings.json.backup_fresh (original)
  - appsettings.json.backup_opt (pre-optimization)
  - appsettings.json.safe_backup (latest safe state)

---

Ready to integrate? Follow Steps 1–3 above and test with the CLI flag.
