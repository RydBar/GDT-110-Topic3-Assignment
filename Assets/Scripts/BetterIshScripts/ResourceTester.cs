// =========================================================================
// ResourceTester.cs
// GDT-110 Activity 2: Resource Loading & Texture Management
// PURPOSE: Tests the ResourceManager by loading textures multiple times
//          and demonstrating that the cache prevents duplicate disk reads.
//          Also provides keyboard controls to unload and clear at runtime.
// =========================================================================
//
// HOW TO USE THIS SCRIPT:
//   1. In Unity, right-click in the Project panel → Create → C# Script.
//   2. Name it exactly "ResourceTester" (must match the class name).
//   3. Paste this code into the file.
//   4. In the Hierarchy, click GameObject → Create Empty.
//   5. Rename the new object to "ResourceTester".
//   6. Drag the script onto that GameObject.
//
// PREREQUISITES:
//   - The ResourceManager script must be attached to a SEPARATE GameObject
//     in the same scene. If ResourceManager is missing, you'll get a
//     NullReferenceException because ResourceManager.Instance will be null.
//   - You must have texture files in Assets/Resources/ that match the
//     names in the texturePaths array below. IMPORTANT: Replace the
//     default names with YOUR actual texture file names!
//
// WHAT THIS SCRIPT DOES:
//   1. Waits 1 second (gives ResourceManager time to initialize).
//   2. Loads each texture 3 times in a loop.
//   3. On the first pass, each texture is loaded from disk (cache miss).
//   4. On passes 2 and 3, textures come from cache (cache hit) — fast!
//   5. Prints statistics showing the cache performance.
//   6. Listens for keyboard input to unload/clear textures at runtime.
//
// RUNTIME CONTROLS:
//   Press U → Unload the first texture from cache
//   Press C → Clear ALL textures from cache
// =========================================================================

using UnityEngine;  // Required for MonoBehaviour, Input, KeyCode, Debug

public class ResourceTester : MonoBehaviour
{
    // =====================================================================
    // TEXTURE PATHS ARRAY
    // =====================================================================
    // This array lists the textures to load during the test.
    // Each string is a path relative to Assets/Resources/.
    //
    // *** IMPORTANT: YOU MUST CHANGE THESE TO MATCH YOUR ACTUAL FILES! ***
    //
    // The "public" keyword means these appear in the Unity Inspector,
    // so you can also change them there without editing code.
    //
    // HOW TO FIND THE CORRECT PATH:
    //   Your file:  Assets/Resources/player_icon.png
    //   Your path:  "player_icon"           ← no extension, no prefix
    //
    //   Your file:  Assets/Resources/Textures/ground.jpg
    //   Your path:  "Textures/ground"       ← include subfolder, no extension
    //
    // If you're unsure, right-click the file in Unity's Project panel,
    // choose "Copy Path", and remove "Assets/Resources/" from the front
    // and the file extension from the end.
    // =====================================================================
    public string[] texturePaths = {
        "squareVar1",       // ← Replace with YOUR texture name
        "squareVar2",    // ← Replace with YOUR texture name
        "squareVar3",       // ← Replace with YOUR texture name
        "squareVar4"       // ← Replace with YOUR texture name
    };

    // =====================================================================
    // Start() — Called once when the scene begins
    // =====================================================================
    void Start()
    {
        Debug.Log("ResourceTester: Starting in 1 second...");
        Debug.Log("Make sure ResourceManager is in the scene on a separate GameObject!");

        // --- SAFETY CHECK ---
        // Verify that ResourceManager exists before we try to use it.
        // If it's null, give the student a clear error message.
        if (ResourceManager.Instance == null)
        {
            Debug.LogError(
                "ResourceTester ERROR: ResourceManager.Instance is null!\n" +
                "  → Did you add the ResourceManager script to a GameObject?\n" +
                "  → Is the ResourceManager GameObject active in the scene?\n" +
                "  → Make sure ResourceManager.cs has no compile errors.");
            return;  // Stop here — don't try to run the test
        }

        // Invoke("RunTest", 1f) calls the RunTest() method after a
        // 1-second delay. This gives all scripts time to initialize.
        // The string "RunTest" must match the method name EXACTLY.
        Invoke("RunTest", 1f);
    }

    // =====================================================================
    // RunTest() — The main test method
    // =====================================================================
    // Loads each texture 3 times and observes cache behavior.
    //
    // EXPECTED RESULTS with 4 textures, 3 passes:
    //   Pass 1: 4 cache MISSES (loads from disk) — these are the first loads
    //   Pass 2: 4 cache HITS (found in cache) — no disk reads!
    //   Pass 3: 4 cache HITS (found in cache) — still no disk reads!
    //   Total:  4 loads from disk + 8 cache hits = 12 requests
    //   Cache hit rate: 8/12 = 66.7%
    //
    // If you see all MISSES (no hits), something is wrong with the cache.
    // If you see errors about textures not found, check your file names.
    // =====================================================================
    void RunTest()
    {
        Debug.Log("========================================");
        Debug.Log("RESOURCE MANAGER PERFORMANCE TEST");
        Debug.Log("========================================");

        // Load each texture 3 times to demonstrate caching
        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"\n--- Load Pass {i + 1} of 3 ---");

            // "foreach" loops through every element in the array
            foreach (string path in texturePaths)
            {
                // This is the key line! We ask the ResourceManager to
                // load each texture. On pass 1, these will be cache misses.
                // On passes 2 and 3, they'll be cache hits.
                Texture2D tex = ResourceManager.Instance.LoadTexture(path);

                // Check if the load was successful
                if (tex == null)
                {
                    Debug.LogWarning($"  Skipping '{path}' — load returned null. " +
                        "Check that this file exists in Assets/Resources/");
                }
            }
        }

        // Print the final statistics
        Debug.Log("\n========================================");
        Debug.Log("TEST COMPLETE — Final Statistics:");
        Debug.Log("========================================");
        ResourceManager.Instance.PrintStats();

        // Remind the student what to look for
        Debug.Log("\nEXPECTED: 4 loads, 8 cache hits, 66.7% hit rate");
        Debug.Log("If your numbers differ, check the troubleshooting section!");
        Debug.Log("\nRUNTIME CONTROLS:");
        Debug.Log("  Press U → Unload first texture from cache");
        Debug.Log("  Press C → Clear entire cache");
    }

    // =====================================================================
    // Update() — Called every frame while the game is running
    // =====================================================================
    // We use this to listen for keyboard input so students can
    // interactively test unloading and clearing the cache.
    //
    // Input.GetKeyDown(KeyCode.X) returns true on the SINGLE FRAME
    // when the key is first pressed down. It won't fire again until
    // the key is released and pressed again.
    // =====================================================================
    void Update()
    {
        // --- Press U to unload the first texture ---
        // This demonstrates selective unloading. The texture is removed
        // from the cache and its memory is freed. If you then try to
        // load it again, it will be a cache miss (loaded from disk).
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("\n--- [U] Unloading first texture ---");

            // Safety check: make sure we have at least one path
            if (texturePaths.Length > 0)
            {
                ResourceManager.Instance.UnloadTexture(texturePaths[0]);
                ResourceManager.Instance.PrintStats();
            }
            else
            {
                Debug.LogWarning("No texture paths configured!");
            }
        }

        // --- Press C to clear the entire cache ---
        // This removes ALL textures. Useful when transitioning between
        // scenes or game states where you need completely different assets.
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("\n--- [C] Clearing entire cache ---");
            ResourceManager.Instance.ClearCache();
            ResourceManager.Instance.PrintStats();
        }

        // --- Press R to reload all textures (re-run the test) ---
        // This lets you see the cache rebuilding after a clear.
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("\n--- [R] Re-running load test ---");
            RunTest();
        }
    }
}
