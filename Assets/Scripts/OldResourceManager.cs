// ResourceManager.cs
using System.Collections.Generic;
using UnityEngine;

public class OldResourceManager : MonoBehaviour
    {
    // Singleton pattern for easy access
    public static OldResourceManager Instance { get; private set; }
    
    // Dictionary to cache loaded textures
    private Dictionary<string, Texture2D> textureCache =
    new Dictionary<string, Texture2D>();

    public Texture2D texture1;
    public Texture2D texture2;
    public Texture2D texture3;
    public Texture2D texture4;

    // Track memory usage
    private long totalTextureMemory = 0;
    private int loadCount = 0;
    private int cacheHitCount = 0;
    void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        textureCache.Add("squareVar1", texture1);
        textureCache.Add("squareVar2", texture2);
        textureCache.Add("squareVar3", texture3);
        textureCache.Add("squareVar4", texture4);
    }

    /// <summary>
    /// Load a texture - uses cache if already loaded
    /// </summary>

    public Texture2D LoadTexture(string path)
    {
        // Check if already loaded
        if (textureCache.ContainsKey(path))
        {
            cacheHitCount++;
            Debug.Log($"✓ Cache HIT: {path} (Total hits: {cacheHitCount})");
            return textureCache[path];
        }
       
        // Not in cache - load from Resources
        Texture2D texture = Resources.Load<Texture2D>(path);
        
        if (texture == null)
        {
            Debug.LogError($"Failed to load texture: {path}");
            return null;
        }
        
        // Add to cache
        textureCache[path] = texture;
        loadCount++;
       
        // Calculate memory used
        long textureMemory = GetTextureMemorySize(texture);
        totalTextureMemory += textureMemory;
        Debug.Log($"★ Loaded NEW texture: {path}");
        Debug.Log($" Size: {texture.width}x{texture.height}");
        Debug.Log($" Memory: {textureMemory / 1024} KB");
        return texture;
    }

    /// <summary>
    /// Unload a specific texture from cache
    /// </summary>
    
    public void UnloadTexture(string path)
    {
        if (textureCache.ContainsKey(path))
        {
            Texture2D texture = textureCache[path];
            long memoryFreed = GetTextureMemorySize(texture);
            Resources.UnloadAsset(texture);
            textureCache.Remove(path);
            totalTextureMemory -= memoryFreed;
            Debug.Log($"✗ Unloaded: {path} (Freed: {memoryFreed / 1024} KB)");
        }
    }

    /// <summary>
    /// Clear all cached textures
    /// </summary>
    
    public void ClearCache()
    {
        foreach (var texture in textureCache.Values)
        {
            Resources.UnloadAsset(texture);
        }
        
        textureCache.Clear();
        totalTextureMemory = 0;
        Debug.Log("Cache cleared!");
    }

    /// <summary>
    /// Calculate memory used by a texture
    /// </summary>
    
    private long GetTextureMemorySize(Texture2D texture)
    {
        // Rough calculation: width * height * bytes per pixel
        // RGBA32 = 4 bytes per pixel
        return texture.width * texture.height * 4;
    }
    
    /// <summary>
    /// Print current memory statistics
    /// </summary>
    
    public void OldPrintStats()
    {
        Debug.Log("=== RESOURCE MANAGER STATS ===");
        Debug.Log($"Textures loaded: {loadCount}");
        Debug.Log($"Cache hits: {cacheHitCount}");
        Debug.Log($"Textures in cache: {textureCache.Count}");
        Debug.Log($"Total texture memory: {totalTextureMemory / 1024 / 1024} MB");
        Debug.Log("==============================");
    }
}