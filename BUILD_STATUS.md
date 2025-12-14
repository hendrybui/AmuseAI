# Amuse Safety Toggle - Build Status

## Current Situation

✅ **BUILD SUCCESSFUL** - The code compiles without errors  
❌ **RUNTIME FAILURE** - App crashes due to ONNX Runtime version mismatch

## The Problem

The built application uses **ONNX Runtime 1.23.0-rc.1** (from NuGet), but the installed Amuse uses **ONNX Runtime 1.23.0 release**. These versions have incompatible ABIs (Application Binary Interfaces), causing:

```
Fatal error. Internal CLR error. (0x80131506)
```

### Technical Details
- **Managed Assembly**: Microsoft.ML.OnnxRuntime.dll v1.23.0-rc.1 (from our build)
- **Native DLL**: onnxruntime.dll v1.23.0 (from installed Amuse)
- **Conflict**: The managed assembly expects different function signatures than the native DLL provides

## Solutions (Choose ONE)

### Option 1: Run from Build Directory (RECOMMENDED)
This avoids mixing versions entirely.

1. **Navigate to build directory**:
   ```powershell
   cd "C:\Users\kentb\repo\AmuseAI\Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0"
   ```

2. **Launch with safety disabled**:
   ```powershell
   .\Amuse.exe --disable-safety
   ```

**ISSUE**: Currently fails because the native onnxruntime.dll in this directory is copied from installed Amuse (v1.23.0) but needs to match the NuGet package version (v1.23.0-rc.1).

**FIX NEEDED**: Download matching native DLL from NuGet package or downgrade the managed package to stable 1.23.0.

### Option 2: Downgrade to Stable ONNX Runtime
Rebuild with the exact version that matches installed Amuse.

1. **Edit Amuse.UI.csproj**, change:
   ```xml
   <PackageReference Include="Microsoft.ML.OnnxRuntime.Managed" Version="1.23.0-rc.1" />
   ```
   To:
   ```xml
   <PackageReference Include="Microsoft.ML.OnnxRuntime.Managed" Version="1.19.0" />
   ```
   (Try different stable versions until one matches the native DLL)

2. **Rebuild**:
   ```powershell
   cd C:\Users\kentb\repo\AmuseAI
   dotnet clean
   dotnet restore
   dotnet build -c Release
   ```

3. **Deploy** (with admin rights):
   ```powershell
   .\deploy-to-program-files.ps1
   ```

### Option 3: Build Self-Contained
Create a standalone build that includes ALL dependencies.

1. **Edit Amuse.UI.csproj**, add:
   ```xml
   <PropertyGroup>
     <SelfContained>true</SelfContained>
     <RuntimeIdentifier>win-x64</RuntimeIdentifier>
   </PropertyGroup>
   ```

2. **Publish**:
   ```powershell
   dotnet publish -c Release --self-contained true -r win-x64
   ```

3. **Run from publish folder**:
   ```powershell
   cd Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0\win-x64\publish
   .\Amuse.exe --disable-safety
   ```

## What's Working

✅ Safety toggle code is complete and integrated:
- `SafetyResolver.cs` - Three-layer toggle resolution (CLI > env > config)
- `SafetyCheckerInitializer.cs` - Guards ONNX ContentFilter initialization  
- `App.xaml.cs` - Startup integration with DI

✅ Build succeeds with all dependencies resolved

✅ Git commits are safe in repository (branch: AmuseA1+)

## What's NOT Working

❌ Runtime crashes due to ONNX Runtime ABI incompatibility  
❌ Cannot mix NuGet dev builds with release native DLLs  
❌ Program Files deployment has permissions issues (appsettings.json access denied)

## Next Steps

**RECOMMENDED PATH**:
1. Try Option 3 (self-contained build) first - this is the cleanest solution
2. If that fails, try Option 2 with different stable ONNX Runtime versions
3. Document the working version for future builds

## Expected Behavior (When Fixed)

When working correctly:
- Memory usage: ~26-30 MB (vs ~634 MB with ContentFilter loaded)
- No crash on startup
- Logs show: "Safety checker disabled via [CLI flag/Environment/Config]"
- NSFW content generation works without filtering

## Test Commands

```powershell
# Test CLI flag
& ".\Amuse.exe" --disable-safety

# Test environment variable
$env:AMUSE_DISABLE_SAFETY = "1"
& ".\Amuse.exe"

# Test config (edit appsettings.json)
# Set: "EnableSafetyChecker": false
& ".\Amuse.exe"
```

## Files Modified

- `Amuse.UI\Services\SafetyResolver.cs` (NEW)
- `Amuse.UI\Services\SafetyCheckerInitializer.cs` (NEW)
- `Amuse.UI\App.xaml.cs` (Modified - DI registration)
- `Amuse.UI\Amuse.UI.csproj` (Modified - removed MIGraphX, font resources)
- `Amuse.UI\Services\ProviderService.cs` (Modified - commented MIGraphX)
- `Amuse.UI\Services\DeviceService.cs` (Modified - commented MIGraphX)

## Build Artifacts

- Location: `C:\Users\kentb\repo\AmuseAI\Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0`
- Binary size: ~2.6 MB (Amuse.dll)
- Executable: 263 KB (Amuse.exe)
- Dependencies: ~50+ DLLs (ONNX Runtime, OnnxStack, Microsoft.Extensions, etc.)

---

**Status**: Build complete, runtime deployment blocked by version conflicts.  
**Date**: December 15, 2025  
**Branch**: AmuseA1+  
**Commits**: 4 commits (safety toggle feature fully implemented)
