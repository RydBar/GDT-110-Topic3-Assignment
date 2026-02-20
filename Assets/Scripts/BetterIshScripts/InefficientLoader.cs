// =========================================================================
// InefficientLoader.cs
// GDT-110 Activity 2: Resource Loading & Texture Management
// PURPOSE: Demonstrates the WRONG way to load textures — used as a
//          baseline so you can compare performance against the
//          ResourceManager approach in later steps.
// =========================================================================
//
// HOW TO USE THIS SCRIPT:
//   1. In Unity, right-click in the Project panel → Create → C# Script.
//   2. Name it exactly "InefficientLoader" (must match the class name).
//   3. Double-click to open in your code editor and paste this code.
//   4. In the Hierarchy, click GameObject → Create Empty.
//   5. Rename the new object to "InefficientLoader".
//   6. Drag the script from the Project panel onto the GameObject in the
//      Hierarchy (or use the Inspector's "Add Component" button).
//   7. Make sure you have at least one texture file named "ground_texture"
//      inside a folder called "Resources" in your Assets folder.
//      Path should be: Assets/Resources/ground_texture.png (or .jpg)
//   8. Press Play and open the Console window (Window → General → Console)
//      to see the output.
//
// IMPORTANT: This script is intentionally inefficient! Do NOT use this
//            pattern in a real game. It exists only so you can see the
//            contrast with the proper ResourceManager.
// =========================================================================

using UnityEngine;  // Gives us access to Unity's core classes like
                    // MonoBehaviour, Debug, Texture2D, Resources, etc.

// -------------------------------------------------------------------------
// CLASS: InefficientLoader
// Inherits from MonoBehaviour so Unity can attach it to a GameObject.
// MonoBehaviour gives us lifecycle methods like Start(), Update(), etc.
// -------------------------------------------------------------------------
public class InefficientLoader : MonoBehaviour
{
    // =====================================================================
    // Start() is called once when the scene begins playing.
    // Unity calls this automatically — you never call Start() yourself.
    // =====================================================================
    void Start()
    {
        // -----------------------------------------------------------------
        // DEMONSTRATION: Loading the same texture 10 times in a loop.
        //
        // WHY THIS IS BAD:
        //   - Each call to Resources.Load() is a disk read operation.
        //   - In a custom loading system (loading from web, files, etc.),
        //     this would create 10 SEPARATE copies in memory.
        //   - Even though Unity's Resources.Load() has some internal
        //     caching, we have NO control over it — we can't track what's
        //     loaded, can't selectively unload, and can't measure usage.
        //   - In a real game with hundreds of textures, this pattern
        //     leads to unpredictable memory usage and slow load times.
        // -----------------------------------------------------------------
        for (int i = 0; i < 10; i++)
        {
            // Resources.Load<Texture2D>("ground_texture") looks for a file at:
            //   Assets/Resources/ground_texture.png  (or .jpg, .tga, etc.)
            //
            // COMMON MISTAKE: Do NOT include the file extension!
            //   WRONG:   Resources.Load<Texture2D>("ground_texture.png")
            //   CORRECT: Resources.Load<Texture2D>("ground_texture")
            //
            // COMMON MISTAKE: Do NOT include "Assets/Resources/" in the path!
            //   WRONG:   Resources.Load<Texture2D>("Assets/Resources/ground_texture")
            //   CORRECT: Resources.Load<Texture2D>("ground_texture")
            //
            // If your texture is in a subfolder inside Resources:
            //   File location: Assets/Resources/Textures/ground_texture.png
            //   Load path:     Resources.Load<Texture2D>("Textures/ground_texture")
            Texture2D texture = Resources.Load<Texture2D>("ground_texture");

            // --- NULL CHECK ---
            // Always check if the load succeeded! If the file doesn't exist
            // at the expected path, texture will be null and accessing
            // texture.width will cause a NullReferenceException.
            if (texture != null)
            {
                Debug.Log($"Loaded texture {i}: {texture.width}x{texture.height}");
            }
            else
            {
                // This error means Unity couldn't find the file.
                // CHECK:
                //   1. Is there a folder called "Resources" inside "Assets"?
                //   2. Is the texture file directly inside that folder?
                //   3. Did you spell the name correctly (case-sensitive)?
                Debug.LogError($"Load attempt {i}: FAILED - texture not found! " +
                    "Check that 'ground_texture' exists in Assets/Resources/");
            }
        }

        // -----------------------------------------------------------------
        // MEMORY MEASUREMENT
        // System.GC.GetTotalMemory(false) returns the approximate number
        // of bytes currently allocated by managed (C#) code.
        //
        // NOTE: This does NOT include all of Unity's native memory
        // (like GPU texture memory). For a more accurate reading in Unity,
        // use the Profiler (Window → Analysis → Profiler), which shows
        // both managed and native memory.
        //
        // The "false" parameter means "don't force garbage collection
        // before measuring." Set to "true" if you want a more accurate
        // (but slower) measurement.
        // -----------------------------------------------------------------
        long memoryUsed = System.GC.GetTotalMemory(false);

        // Convert bytes → megabytes for readability.
        // 1 MB = 1024 KB = 1024 * 1024 bytes = 1,048,576 bytes
        // We divide by 1024 twice: bytes → KB → MB
        float memoryMB = memoryUsed / 1024f / 1024f;
        Debug.Log($"Approximate managed memory used: {memoryMB:F2} MB");

        // TIP: Write down this number! You'll compare it against the
        //      ResourceManager approach in Step 4.
        Debug.Log("=== BASELINE TEST COMPLETE ===");
        Debug.Log("Now disable this script and try the ResourceManager approach.");
    }
}
