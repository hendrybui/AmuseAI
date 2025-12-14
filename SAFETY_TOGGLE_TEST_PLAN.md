# Safety Toggle Feature - Testing Plan

## Current Status
- **Installed Version**: Amuse 3.1.0 (no toggle code - original build)
- **Fork Version**: AmuseA1+ branch with safety toggle code
- **Code Status**: Ready for compilation
- **Compilation Status**: Pending .NET SDK 8.0+ installation

## Test Scenarios

Once the forked version is built with .NET SDK 8.0+, run these tests:

### Test 1: CLI Flag (Highest Priority)
```bash
cd C:\Users\kentb\repo\AmuseAI
# Build first
dotnet build -c Release

# After build, run with CLI flag
.\bin\Release\net8.0-windows\Amuse.exe --disable-safety
```
**Expected Behavior**:
- Safety checker DISABLED
- ContentFilter.onnx NOT loaded
- Logs should show: "Safety checker disabled (source: CLI)"
- Application should run stable

### Test 2: Environment Variable
```bash
$env:AMUSE_DISABLE_SAFETY=1
.\bin\Release\net8.0-windows\Amuse.exe
```
**Expected Behavior**:
- Safety checker DISABLED
- ContentFilter.onnx NOT loaded
- Logs should show: "Safety checker disabled (source: Environment)"
- Application should run stable

### Test 3: Configuration File
**File**: `$env:LOCALAPPDATA\Amuse\appsettings.json`

Add to AmuseSettings section:
```json
"EnableSafetyChecker": false,
```

Then run:
```bash
.\bin\Release\net8.0-windows\Amuse.exe
```
**Expected Behavior**:
- Safety checker DISABLED
- ContentFilter.onnx NOT loaded
- Logs should show: "Safety checker disabled (source: Config)"
- Application should run stable

### Test 4: Priority Order (All Methods Together)
```bash
# Config: EnableSafetyChecker = false
$env:AMUSE_DISABLE_SAFETY=0  # Set to false
.\bin\Release\net8.0-windows\Amuse.exe --disable-safety
```
**Expected Behavior**:
- CLI flag takes priority
- Logs should show: "Safety checker disabled (source: CLI)"
- Even though env var would enable it, CLI disables it

### Test 5: Default Behavior (Nothing Set)
**Config**: Remove EnableSafetyChecker setting or set to `true`
**Env var**: Not set
**CLI flag**: Not used

```bash
.\bin\Release\net8.0-windows\Amuse.exe
```
**Expected Behavior**:
- Safety checker ENABLED
- ContentFilter.onnx IS loaded
- May crash on RX 580 (original issue - expected)
- Logs should show: "Safety checker enabled (source: Default)"

## Success Criteria

✓ CLI flag works and takes priority over other methods
✓ Environment variable works when CLI flag not used
✓ Config setting works when env var not used
✓ Logs clearly indicate source of toggle state
✓ Safety checker gracefully skips ONNX init when disabled
✓ No crashes when safety checker disabled
✓ All three methods prevent ContentFilter.onnx loading

## Installation Instructions

### Step 1: Install .NET SDK 8.0+
Download from: https://dotnet.microsoft.com/download

Verify installation:
```bash
dotnet --version
# Should show: 8.0.x or higher
```

### Step 2: Build the Solution
```bash
cd C:\Users\kentb\repo\AmuseAI
dotnet build -c Release
```

Expected output:
```
Build succeeded in X.XXs
```

### Step 3: Run Tests
Execute test commands above.

## Notes

- Current Amuse 3.1.0 crashes with error `0xe0434352` (.NET runtime exception) - unrelated to safety toggle
- This is likely a hardware/VRAM issue, not the toggle mechanism
- Once toggle code is built, it should gracefully skip ContentFilter initialization
- All toggle logic is already implemented in:
  - `SafetyResolver.cs`: Resolves toggle state
  - `SafetyCheckerInitializer.cs`: Guards ONNX initialization
  - `App.xaml.cs`: DI registration and startup guard

## Repository
Branch: `AmuseA1+`
Commits: b435ae6 (services), 12c54e3 (App.xaml.cs integration)
