// =========================================================================
// ResourceManager.cs
// GDT-110 Activity 2: Resource Loading & Texture Management
// PURPOSE: A proper resource management system that caches loaded textures,
//          prevents duplicate loading, tracks memory usage, and provides
//          controlled unloading. This is the CORE script of the activity.
// =========================================================================
//
// HOW TO USE THIS SCRIPT:
//   1. In Unity, right-click in the Project panel → Create → C# Script.
//   2. Name it exactly "ResourceManager" (must match the class name).
//   3. Double-click to open in your code editor and paste this code.
//   4. In the Hierarchy, click GameObject → Create Empty.
//   5. Rename the new object to "ResourceManager".
//   6. Drag the script onto that GameObject.
//   7. This script uses the Singleton pattern, meaning there should be
//      exactly ONE ResourceManager in your scene. Other scripts access
//      it via: ResourceManager.Instance.LoadTexture("myTexture");
//
// PREREQUISITES:
//   - A folder named "Resources" inside your Assets folder.
//   - Texture files (.png or .jpg) inside that Resources folder.
//   - Do NOT include file extensions when loading (Unity handles this).
//
// WHAT THIS SCRIPT DOES:
//   - Maintains a Dictionary (like a lookup table) that maps texture
//     names to already-loaded Texture2D objects.
//   - When you ask for a texture, it checks the Dictionary first.
//     If found → returns the cached copy (fast, no disk read).
//     If not found → loads from disk, adds to Dictionary, returns it.
//   - Tracks statistics: how many textures loaded, cache hits, memory.
//   - Provides methods to unload individual textures or clear everything.
// =========================================================================

using System.Collections.Generic;  // Required for Dictionary<TKey, TValue>
using UnityEngine;                 // Required for MonoBehaviour, Texture2D, etc.

// -------------------------------------------------------------------------
// CLASS: ResourceManager
// This is a Singleton MonoBehaviour — only one instance exists at a time,
// and it persists across scene changes (DontDestroyOnLoad).
// -------------------------------------------------------------------------
public class ResourceManager : MonoBehaviour
{
    // =====================================================================
    // SINGLETON PATTERN
    // =====================================================================
    // "public static" means any script can access this without needing
    // a reference to the GameObject. Just use: ResourceManager.Instance
    //
    // "{ get; private set; }" means:
    //   - Any script can READ Instance (get)
    //   - Only THIS script can WRITE to Instance (private set)
    //
    // WHY USE A SINGLETON?
    //   In a game, you typically want exactly ONE resource manager that
    //   all systems share. If you had multiple, each would have its own
    //   cache, defeating the purpose of preventing duplicate loads.
    // =====================================================================
    public static ResourceManager Instance { get; private set; }

    // =====================================================================
    // TEXTURE CACHE
    // =====================================================================
    // A Dictionary is like a real-world dictionary:
    //   - The "key" is the word you look up (here, the texture path string)
    //   - The "value" is the definition (here, the Texture2D object)
    //
    // Looking up a key in a Dictionary is very fast (O(1) on average),
    // much faster than searching through a list.
    //
    // Example contents after loading:
    //   "player_icon"    → [Texture2D object, 256x256]
    //   "ground_texture" → [Texture2D object, 1024x1024]
    //   "skybox_side"    → [Texture2D object, 2048x2048]
    // =====================================================================
    private Dictionary<string, Texture2D> textureCache =
        new Dictionary<string, Texture2D>();

    // =====================================================================
    // STATISTICS TRACKING
    // =====================================================================
    // These help you measure how well the cache is performing.
    //
    // totalTextureMemory: Running total of bytes used by cached textures.
    //                     Goes up when you load, down when you unload.
    //
    // loadCount:          How many unique textures were loaded from disk.
    //                     Only increments on a cache MISS (first load).
    //
    // cacheHitCount:      How many times a requested texture was already
    //                     in the cache. Higher = better performance!
    // =====================================================================
    private long totalTextureMemory = 0;
    private int loadCount = 0;
    private int cacheHitCount = 0;

    // =====================================================================
    // Awake() runs before Start(). We set up the Singleton here so it's
    // ready before any other script's Start() tries to use it.
    //
    // EXECUTION ORDER: Awake() → OnEnable() → Start()
    // This means if ResourceTester.Start() calls ResourceManager.Instance,
    // the Instance is guaranteed to exist (set in Awake).
    // =====================================================================
    void Awake()
    {
        // --- SINGLETON SETUP ---
        // First time: Instance is null, so we set it to this object.
        // If the scene reloads or another copy exists: Instance is already
        // set, so we destroy the duplicate.
        if (Instance == null)
        {
            Instance = this;

            // DontDestroyOnLoad keeps this GameObject alive when you
            // load a new scene. Without it, the ResourceManager would
            // be destroyed on scene change and all cached textures lost.
            DontDestroyOnLoad(gameObject);

            Debug.Log("ResourceManager initialized and ready.");
        }
        else
        {
            // A second ResourceManager was found — destroy it!
            Debug.LogWarning("Duplicate ResourceManager destroyed. " +
                "Only one instance is allowed.");
            Destroy(gameObject);
        }
    }

    // =====================================================================
    // PUBLIC METHOD: LoadTexture
    // =====================================================================
    // Call this from any script to load a texture:
    //   Texture2D tex = ResourceManager.Instance.LoadTexture("player_icon");
    //
    // PARAMETERS:
    //   path - The name of the texture file inside Assets/Resources/
    //          Do NOT include file extension or "Assets/Resources/" prefix.
    //          Examples: "player_icon", "Textures/ground", "UI/button_bg"
    //
    // RETURNS:
    //   The loaded Texture2D, or null if the file wasn't found.
    //
    // HOW IT WORKS:
    //   1. Check if 'path' already exists as a key in textureCache.
    //   2. If YES (cache hit)  → return the cached texture. Fast!
    //   3. If NO  (cache miss) → load from disk, add to cache, return it.
    // =====================================================================
    public Texture2D LoadTexture(string path)
    {
        // -----------------------------------------------------------------
        // STEP 1: CHECK THE CACHE
        // ContainsKey() checks if the Dictionary already has this path.
        // This is an O(1) operation — nearly instant.
        // -----------------------------------------------------------------
        if (textureCache.ContainsKey(path))
        {
            // Cache HIT — the texture was already loaded before.
            // We just return the existing reference. No disk read needed!
            cacheHitCount++;
            Debug.Log($"✓ Cache HIT: {path} (Total hits: {cacheHitCount})");
            return textureCache[path];
        }

        // -----------------------------------------------------------------
        // STEP 2: LOAD FROM DISK (cache miss)
        // This is the expensive operation we want to minimize.
        // Resources.Load reads the file from disk and creates a Texture2D.
        // -----------------------------------------------------------------
        Texture2D texture = Resources.Load<Texture2D>(path);

        // -----------------------------------------------------------------
        // STEP 3: NULL CHECK — did the load succeed?
        // -----------------------------------------------------------------
        if (texture == null)
        {
            // The file wasn't found. Common causes:
            //   - Typo in the path name
            //   - File is not in Assets/Resources/ folder
            //   - File extension was included (don't do that)
            //   - Case mismatch (Unity paths are case-sensitive on some platforms)
            Debug.LogError($"✗ FAILED to load texture: '{path}'\n" +
                "  → Check that the file exists at: Assets/Resources/{path}.png (or .jpg)\n" +
                "  → Make sure you did NOT include the file extension in the path.\n" +
                "  → Verify the spelling and capitalization match exactly.");
            return null;
        }

        // -----------------------------------------------------------------
        // STEP 4: ADD TO CACHE
        // Store the loaded texture in our Dictionary so future requests
        // for the same path will get a cache hit.
        // -----------------------------------------------------------------
        textureCache[path] = texture;
        loadCount++;

        // -----------------------------------------------------------------
        // STEP 5: TRACK MEMORY USAGE
        // Calculate how much memory this texture uses and add it to our
        // running total.
        // -----------------------------------------------------------------
        long textureMemory = GetTextureMemorySize(texture);
        totalTextureMemory += textureMemory;

        // Log details about the newly loaded texture
        Debug.Log($"★ Loaded NEW texture: {path}");
        Debug.Log($"  Size: {texture.width}x{texture.height}");
        Debug.Log($"  Memory: {textureMemory / 1024} KB");

        return texture;
    }

    // =====================================================================
    // PUBLIC METHOD: UnloadTexture
    // =====================================================================
    // Removes a specific texture from the cache and frees its memory.
    // Call when you know a texture is no longer needed (e.g., leaving a level).
    //
    // PARAMETERS:
    //   path - The same path string used in LoadTexture.
    //
    // USAGE:
    //   ResourceManager.Instance.UnloadTexture("player_icon");
    //
    // IMPORTANT: After unloading, any Materials or UI elements still
    //            referencing this texture will show pink/missing texture!
    //            Only unload when you're sure nothing is using it.
    // =====================================================================
    public void UnloadTexture(string path)
    {
        if (textureCache.ContainsKey(path))
        {
            // Get the texture reference before removing it
            Texture2D texture = textureCache[path];

            // Calculate how much memory we're freeing
            long memoryFreed = GetTextureMemorySize(texture);

            // Resources.UnloadAsset() tells Unity to release the native
            // (GPU) memory used by this asset. This is the actual
            // memory cleanup — just removing from the Dictionary wouldn't
            // free the GPU memory.
            Resources.UnloadAsset(texture);

            // Remove from our tracking Dictionary
            textureCache.Remove(path);

            // Update our running memory total
            totalTextureMemory -= memoryFreed;

            Debug.Log($"✗ Unloaded: {path} (Freed: {memoryFreed / 1024} KB)");
        }
        else
        {
            Debug.LogWarning($"Cannot unload '{path}' — it's not in the cache. " +
                "Was it already unloaded, or was it never loaded?");
        }
    }

    // =====================================================================
    // PUBLIC METHOD: ClearCache
    // =====================================================================
    // Removes ALL textures from the cache. Use when transitioning between
    // major game sections (e.g., going from Main Menu to Gameplay).
    //
    // USAGE:
    //   ResourceManager.Instance.ClearCache();
    // =====================================================================
    public void ClearCache()
    {
        // Unload each texture from Unity's native memory
        foreach (var texture in textureCache.Values)
        {
            Resources.UnloadAsset(texture);
        }

        // Clear the Dictionary (removes all key-value pairs)
        textureCache.Clear();

        // Reset memory counter
        totalTextureMemory = 0;

        Debug.Log("Cache cleared! All textures unloaded from memory.");
    }

    // =====================================================================
    // PRIVATE METHOD: GetTextureMemorySize
    // =====================================================================
    // Estimates the memory used by a texture in bytes.
    //
    // FORMULA: width × height × bytesPerPixel
    //
    // For an RGBA32 texture (the most common uncompressed format):
    //   - R (red)   = 1 byte
    //   - G (green) = 1 byte
    //   - B (blue)  = 1 byte
    //   - A (alpha) = 1 byte
    //   - Total     = 4 bytes per pixel
    //
    // EXAMPLE:
    //   A 1024×1024 RGBA32 texture uses:
    //   1024 * 1024 * 4 = 4,194,304 bytes = 4 MB
    //
    // NOTE: This is a rough estimate. Compressed textures (DXT, ETC, ASTC)
    //       use much less memory. For exact values, use Unity's Profiler.
    // =====================================================================
    private long GetTextureMemorySize(Texture2D texture)
    {
        // Rough calculation: width * height * bytes per pixel
        // RGBA32 = 4 bytes per pixel
        return texture.width * texture.height * 4;
    }

    // =====================================================================
    // PUBLIC METHOD: PrintStats
    // =====================================================================
    // Outputs current performance statistics to the Console.
    // Use this to verify your cache is working correctly.
    //
    // WHAT THE NUMBERS MEAN:
    //   - "Textures loaded" = unique textures read from disk (cache misses)
    //   - "Cache hits" = times a texture was found already in cache
    //   - "Textures in cache" = how many are currently stored
    //   - "Total texture memory" = estimated bytes used by cached textures
    //
    // IDEAL RESULTS:
    //   Cache hits should be much higher than textures loaded!
    //   If you load 4 textures 3 times each:
    //     Textures loaded = 4 (first pass)
    //     Cache hits = 8 (second + third pass)
    //     Hit rate = 8 / 12 = 66.7%
    // =====================================================================
    public void PrintStats()
    {
        Debug.Log("=== RESOURCE MANAGER STATS ===");
        Debug.Log($"Textures loaded from disk: {loadCount}");
        Debug.Log($"Cache hits (avoided reloads): {cacheHitCount}");
        Debug.Log($"Textures currently in cache: {textureCache.Count}");
        Debug.Log($"Total texture memory: {totalTextureMemory / 1024f / 1024f:F2} MB");

        // Calculate and display hit rate percentage
        int totalRequests = loadCount + cacheHitCount;
        if (totalRequests > 0)
        {
            float hitRate = (float)cacheHitCount / totalRequests * 100f;
            Debug.Log($"Cache hit rate: {hitRate:F1}%");
        }

        Debug.Log("==============================");
    }

    // =====================================================================
    // PUBLIC METHOD: GetCacheCount (HELPER)
    // =====================================================================
    // Returns the number of textures currently stored in the cache.
    // Useful for the MemoryMonitor UI display.
    // =====================================================================
    public int GetCacheCount()
    {
        return textureCache.Count;
    }

    // =====================================================================
    // PUBLIC METHOD: GetTotalMemory (HELPER)
    // =====================================================================
    // Returns the estimated total memory used by cached textures in bytes.
    // Useful for the MemoryMonitor UI display.
    // =====================================================================
    public long GetTotalMemory()
    {
        return totalTextureMemory;
    }
}
