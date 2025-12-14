# How to Build and Install Modified AmuseAI

## Prerequisites

You need:
- **Visual Studio 2022** (Community Edition is free)
- **.NET 8.0 SDK** installed
- **Windows 10/11** (64-bit)

## Build Instructions

### Step 1: Clone Your Fork (if not already done)
```bash
git clone https://github.com/hendrybui/AmuseAI.git
cd AmuseAI
git checkout -b amuseai
```

### Step 2: Apply the Changes
The changes are already made in the GitHub virtual workspace. If you're working locally:
1. Pull the changes from your fork, OR
2. Manually apply changes from `OPTIMIZATION_CHANGES.md`

### Step 3: Build the Solution

#### Using Visual Studio (Easiest):
1. Open `Amuse.sln` in Visual Studio 2022
2. Select **Release** configuration (top toolbar)
3. Select **x64** platform
4. Right-click the solution → **Rebuild Solution**
5. Wait for build to complete (may take 5-10 minutes)

#### Using Command Line:
```powershell
# Navigate to the solution folder
cd C:\path\to\AmuseAI

# Build in Release mode
dotnet build Amuse.sln -c Release -p:Platform=x64

# Or use MSBuild directly
msbuild Amuse.sln /p:Configuration=Release /p:Platform=x64
```

### Step 4: Find the Built Executable

After building, the executable will be in:
```
Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0\Amuse.exe
```

## Installation Options

### Option A: Replace Existing Installation (Recommended)

1. **Locate your current AmuseAI installation:**
   - Usually in: `C:\Program Files\Amuse\`
   - Or check registry: `HKLM\SOFTWARE\Amuse\Install_Dir`

2. **Backup the original:**
   ```powershell
   # Backup existing installation
   Copy-Item "C:\Program Files\Amuse" "C:\Program Files\Amuse.backup" -Recurse
   ```

3. **Stop AmuseAI if running:**
   - Close the application
   - Check Task Manager for any running processes

4. **Replace files:**
   ```powershell
   # Copy new build (Run as Administrator)
   Copy-Item "Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0\*" `
             "C:\Program Files\Amuse\" -Recurse -Force
   ```

5. **Launch AmuseAI:**
   - Use your normal shortcut
   - Or run: `C:\Program Files\Amuse\Amuse.exe`

### Option B: Standalone Installation (Testing)

1. **Create a new folder:**
   ```powershell
   mkdir "C:\AmuseAI-Modified"
   ```

2. **Copy all build output:**
   ```powershell
   Copy-Item "Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0\*" `
             "C:\AmuseAI-Modified\" -Recurse
   ```

3. **Copy Plugins folder (IMPORTANT!):**
   ```powershell
   # From your existing installation
   Copy-Item "C:\Program Files\Amuse\Plugins" `
             "C:\AmuseAI-Modified\Plugins" -Recurse
   ```

4. **Run from new location:**
   ```powershell
   cd C:\AmuseAI-Modified
   .\Amuse.exe
   ```

### Option C: Run Directly from Build (Development)

**Just for testing - don't use this for regular use:**

1. In Visual Studio, press **F5** to run
2. Or run from PowerShell:
   ```powershell
   cd Amuse.UI\bin\x64\Release\net8.0-windows10.0.17763.0
   .\Amuse.exe
   ```

## Verifying the Changes

After launching, verify the optimizations:

### 1. NSFW Filter Disabled:
- Go to **Settings** → Check that "Content Moderation" is **unchecked**
- Try generating content that would normally be filtered
- No warning dialogs should appear

### 2. Performance Settings:
- Settings should show:
  - Model Cache Mode: **Multi**
  - Render Mode: **Default** (hardware accelerated)
  - Realtime Refresh: **100ms**

### 3. Check Runtime Config (Advanced):
Open Task Manager while AmuseAI is running:
- Should show multi-threaded operation
- GPU usage should be visible (if you have a discrete GPU)

## Troubleshooting

### Build Errors:

**Missing .NET 8.0 SDK:**
```bash
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
```

**Missing Dependencies:**
```powershell
# Restore NuGet packages
dotnet restore Amuse.sln
```

### Runtime Errors:

**"Cannot find Plugins folder":**
- Copy the Plugins folder from your original installation
- Should be in: `C:\Program Files\Amuse\Plugins`

**"DirectML device not found":**
- The optimizations require DirectML-capable GPU
- Fallback to CPU will still work but slower

**Application crashes on startup:**
- Revert to original `appdefaults.json` from backup
- Try changing `ModelCacheMode` back to `Single` if low on RAM

## Reverting to Original

### Quick Revert:
```powershell
# Restore from backup
Remove-Item "C:\Program Files\Amuse" -Recurse -Force
Copy-Item "C:\Program Files\Amuse.backup" "C:\Program Files\Amuse" -Recurse
```

### Clean Install:
1. Uninstall AmuseAI from Windows Settings
2. Reinstall from the original installer

## Creating an Installer (Advanced)

If you want to create a proper installer for your modified version:

1. Install **WiX Toolset** or **Inno Setup**
2. Create installer configuration
3. Package the build output
4. Note: This is complex - only do if you plan to distribute

## Performance Comparison

To see if optimizations are working:

**Before (Original):**
- Generation time: ~X seconds
- UI refresh: 50ms intervals
- Single model in memory

**After (Optimized):**
- Generation time: Should be 10-20% faster
- UI refresh: 100ms (smoother)
- Multiple models cached
- Better GPU utilization

Use the same prompt/settings to compare!

---

## Summary

**Easiest Path:**
1. Build in Visual Studio (Release/x64)
2. Backup `C:\Program Files\Amuse`
3. Copy new build over existing installation
4. Launch and test

**Safest Path:**
1. Build in Visual Studio
2. Copy to `C:\AmuseAI-Modified`
3. Copy Plugins folder
4. Test from new location first
5. If working well, replace main installation
