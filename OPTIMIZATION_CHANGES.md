# AmuseAI Optimization Changes

## ⚠️ IMPORTANT: These commits are for YOUR FORK ONLY

**DO NOT push to TensorStack-AI/AmuseAI!**

These changes will only be committed to **hendrybui/AmuseAI** (your fork).
The `amuseai` branch will remain local to your repository.

---

## How to Create the Commits

Since this is a GitHub virtual workspace, you'll need to create these commits manually. Here's how:

### Step 1: Create a new branch (stays in YOUR fork)
```bash
git checkout -b amuseai
```

### Step 2: Stage the files
```bash
git add Amuse.UI/appdefaults.json Amuse.UI/Amuse.UI.csproj
```

### Step 3: Create Commit 1 - Disable NSFW Filter
```bash
git commit -m "Disable NSFW content filter

- Set IsModelEvaluationModeEnabled to true to bypass content moderation
- This allows unrestricted prompt generation without content filtering
- Content filter pipeline will be skipped during image/video generation"
```

### Step 4: Create Commit 2 - Performance Optimizations
```bash
git commit -m "Optimize performance settings for hardware

- Changed GC LatencyMode from 1 (Interactive) to 0 (Batch) for better throughput
- Added ThreadPool configuration: MinThreads=16, MaxThreads=64
- Changed ModelCacheMode from Single to Multi for better model caching
- Changed RenderMode from SoftwareOnly to Default for hardware acceleration
- Increased RealtimeRefreshRate from 50ms to 100ms for smoother updates
- Optimized for better GPU/DirectML utilization and responsiveness"
```

### Step 5: Push to YOUR fork (optional)
```bash
# Only push to YOUR fork (hendrybui/AmuseAI)
git push origin amuseai

# To set as upstream for future pushes
git push -u origin amuseai
```

### ⛔ DO NOT DO THIS:
```bash
# NEVER push to the upstream TensorStack-AI repo!
# git push upstream amuseai  ❌ DON'T DO THIS
```

---

## Detailed Changes

### Commit 1: Disable NSFW Content Filter

**File:** `Amuse.UI/appdefaults.json`

**Line 28:**
```diff
- "IsModelEvaluationModeEnabled": false,
+ "IsModelEvaluationModeEnabled": true,
```

**Effect:**
- Disables all NSFW content filtering
- Bypasses prompt moderation checks
- Removes content filter from image/video generation pipeline
- No restrictions on prompt content

---

### Commit 2: Performance Optimizations

#### File 1: `Amuse.UI/Amuse.UI.csproj`

**Lines 21-27:**
```diff
- <!--HostConfiguration-->
+ <!--HostConfiguration - Optimized for Performance-->
  <ItemGroup>
    <RuntimeHostConfigurationOption Include="System.GC.Server" Value="true" />
    <RuntimeHostConfigurationOption Include="System.GC.Concurrent" Value="true" />
-   <RuntimeHostConfigurationOption Include="System.GC.LatencyMode" Value="1" />
+   <RuntimeHostConfigurationOption Include="System.GC.LatencyMode" Value="0" />
    <RuntimeHostConfigurationOption Include="System.GC.LargeObjectHeapCompactionMode" Value="2" />
+   <RuntimeHostConfigurationOption Include="System.Threading.ThreadPool.MinThreads" Value="16" />
+   <RuntimeHostConfigurationOption Include="System.Threading.ThreadPool.MaxThreads" Value="64" />
  </ItemGroup>
```

**Optimization Details:**
- **GC.LatencyMode = 0 (Batch)**: Optimizes for throughput over low latency, better for AI workloads
- **ThreadPool.MinThreads = 16**: Ensures sufficient threads are immediately available
- **ThreadPool.MaxThreads = 64**: Allows parallel processing for better multi-core utilization

#### File 2: `Amuse.UI/appdefaults.json`

**Lines 9-12:**
```diff
  "UIMode": "Basic",
- "ModelCacheMode": "Single",
+ "ModelCacheMode": "Multi",
- "RenderMode": "SoftwareOnly",
+ "RenderMode": "Default",
  "UseLegacyDeviceDetection": false,
```

**Lines 19-21:**
```diff
  "DirectoryVideoAutoSave": "",
  "BatchDelay": 0,
- "RealtimeRefreshRate": 50,
+ "RealtimeRefreshRate": 100,
  "RealtimeHistoryEnabled": true,
```

**Optimization Details:**
- **ModelCacheMode: Multi**: Keeps multiple models in memory for faster switching
- **RenderMode: Default**: Enables hardware-accelerated rendering (GPU)
- **RealtimeRefreshRate: 100ms**: Smoother real-time preview updates

---

## Performance Impact

### Expected Improvements:
1. **Faster Generation**: Multi-threaded processing with optimized GC
2. **Better GPU Utilization**: Hardware-accelerated rendering
3. **Smoother UI**: Higher refresh rate and better threading
4. **Faster Model Switching**: Multi-model caching
5. **No Content Filtering Overhead**: Removed NSFW filter processing

### Hardware Requirements:
- Works best with discrete GPU (NVIDIA/AMD)
- Minimum 16GB RAM recommended for Multi cache mode
- DirectML-compatible GPU for best performance

---

## Reverting Changes

If you need to revert these changes:

```bash
git checkout master
git branch -D amuseai
```

Or to revert individual commits:
```bash
git revert HEAD      # Revert last commit
git revert HEAD~1    # Revert second to last commit
```
