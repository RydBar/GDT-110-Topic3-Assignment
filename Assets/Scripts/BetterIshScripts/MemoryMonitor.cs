// =========================================================================
// MemoryMonitor.cs
// GDT-110 Activity 2: Resource Loading & Texture Management
// PURPOSE: Displays a real-time on-screen overlay showing memory usage,
//          FPS, and cache statistics. This gives you a visual way to see
//          the impact of loading and unloading textures.
// =========================================================================
//
// HOW TO USE THIS SCRIPT (Step-by-Step):
//
//   STEP A — Create the Canvas (the container for all UI elements):
//     1. In the Hierarchy panel, click: GameObject → UI → Canvas
//     2. A "Canvas" object will appear in the Hierarchy.
//        (An "EventSystem" will also be created automatically — leave it.)
//     3. In the Inspector for the Canvas, make sure:
//        - Render Mode = "Screen Space - Overlay"
//        - This is the default, so you shouldn't need to change it.
//
//   STEP B — Create the Text element:
//     1. Right-click on the Canvas in the Hierarchy.
//     2. Choose: UI → Text (or UI → Legacy → Text if using newer Unity).
//        NOTE: If you see "TextMeshPro" options, you can use those too,
//        but you'll need to change "using UnityEngine.UI" to
//        "using TMPro" and change "Text" to "TextMeshProUGUI".
//     3. Rename the Text object to "MemoryDisplay".
//
//   STEP C — Position the Text in the top-left corner:
//     1. Select the "MemoryDisplay" Text object.
//     2. In the Inspector, find the Rect Transform component.
//     3. Click the Anchor Presets button (square icon in top-left of
//        Rect Transform). Hold Alt (Option on Mac) and click the
//        top-left anchor preset. This anchors AND positions the text
//        to the top-left corner.
//     4. Set Width = 400, Height = 200 (enough room for our text).
//     5. In the Text component, set:
//        - Font Size: 14–18 (your choice)
//        - Color: White or bright green (so it's visible)
//        - Alignment: Left
//        - Overflow: Overflow (so text isn't clipped)
//
//   STEP D — Attach this script:
//     1. Create a new C# script called "MemoryMonitor".
//     2. Paste this code into it.
//     3. You can attach it to the Canvas, the Text object, or a
//        separate empty GameObject — it doesn't matter which.
//     4. In the Inspector for the script, you'll see a "Display Text"
//        field. Drag the "MemoryDisplay" Text object from the Hierarchy
//        into that field.
//
//   STEP E — Test it:
//     1. Press Play.
//     2. You should see memory stats in the top-left corner of the
//        Game view, updating every 0.5 seconds.
//
// TROUBLESHOOTING:
//   - "Display Text" field is empty / text doesn't appear:
//     → You forgot to drag the Text object into the script's field.
//     → Or the Text object is disabled or hidden behind other UI.
//
//   - NullReferenceException on displayText:
//     → The displayText field was not assigned in the Inspector.
//     → Fix: Select the GameObject with this script, then drag the
//       Text object from the Hierarchy into the "Display Text" slot.
//
//   - Text appears but shows nothing or garbled characters:
//     → Make sure the Text component's Font is assigned (usually
//       "Arial" by default). If it's missing, drag any font into it.
//
//   - Text is too small / too large:
//     → Adjust the Font Size in the Text component (not in this script).
//
//   - Canvas doesn't show in Game view:
//     → Check that Render Mode is "Screen Space - Overlay".
//     → Make sure the Canvas and Text objects are active (checkbox).
// =========================================================================

using UnityEngine;     // For MonoBehaviour, Time, Debug, etc.
using UnityEngine.UI;  // For the Text component (Unity's built-in UI)
                       // NOTE: If using TextMeshPro, change this to:
                       //   using TMPro;
                       // and change "Text" to "TextMeshProUGUI" below.

public class MemoryMonitor : MonoBehaviour
{
    // =====================================================================
    // PUBLIC FIELDS (visible in Unity Inspector)
    // =====================================================================

    // Drag your UI Text object here in the Inspector.
    // This is the Text component where we'll display the stats.
    //
    // If using TextMeshPro, change this line to:
    //   public TextMeshProUGUI displayText;
    public Text displayText;

    // How often to update the display, in seconds.
    // 0.5 means twice per second. Lower = more frequent updates but
    // slightly more CPU usage. For a debug display, 0.5 is fine.
    // You can change this in the Inspector without editing code.
    public float updateInterval = 0.5f;

    // =====================================================================
    // PRIVATE FIELDS
    // =====================================================================
    private float timer;           // Tracks time since last update
    private float deltaTime = 0f;  // Smoothed delta time for FPS calculation

    // =====================================================================
    // Start() — Validate setup on launch
    // =====================================================================
    void Start()
    {
        // Check that the student remembered to assign the Text reference
        if (displayText == null)
        {
            Debug.LogError(
                "MemoryMonitor ERROR: 'Display Text' is not assigned!\n" +
                "  → Select this GameObject in the Hierarchy.\n" +
                "  → In the Inspector, find the MemoryMonitor component.\n" +
                "  → Drag your UI Text object into the 'Display Text' field.");
        }
    }

    // =====================================================================
    // Update() — Called every frame
    // =====================================================================
    // We use a timer so we don't update the text EVERY frame (which
    // would be wasteful for a debug display). Instead, we update at
    // the interval specified by updateInterval.
    // =====================================================================
    void Update()
    {
        // Smooth the deltaTime for a more stable FPS reading.
        // This uses a simple exponential moving average.
        // Without smoothing, the FPS number would jump around wildly.
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Accumulate time since last display update
        timer += Time.deltaTime;

        // Only update the display at the specified interval
        if (timer >= updateInterval)
        {
            UpdateDisplay();
            timer = 0f;  // Reset the timer
        }
    }

    // =====================================================================
    // UpdateDisplay() — Builds and shows the stats text
    // =====================================================================
    void UpdateDisplay()
    {
        // Safety check — don't try to set text on a null reference
        if (displayText == null) return;

        // -----------------------------------------------------------------
        // MANAGED MEMORY
        // System.GC.GetTotalMemory(false) returns bytes used by C# objects.
        // We convert to MB for readability.
        //
        // NOTE: This is only MANAGED memory (C# heap). Unity also uses
        // NATIVE memory for textures, meshes, audio, etc. For the full
        // picture, use the Unity Profiler (Window → Analysis → Profiler).
        // -----------------------------------------------------------------
        long totalMemory = System.GC.GetTotalMemory(false);
        float memoryMB = totalMemory / 1024f / 1024f;

        // -----------------------------------------------------------------
        // FPS CALCULATION
        // FPS = 1 / deltaTime
        // We use the smoothed deltaTime for a stable reading.
        // -----------------------------------------------------------------
        float fps = 1.0f / deltaTime;

        // -----------------------------------------------------------------
        // BUILD THE DISPLAY STRING
        // The $ before the string enables string interpolation:
        //   $"Value: {variable}" inserts the variable's value.
        //   $"Value: {variable:F2}" formats it to 2 decimal places.
        //   \n = new line
        // -----------------------------------------------------------------
        string info = $"=== Memory Monitor ===\n";
        info += $"Managed Memory: {memoryMB:F2} MB\n";
        info += $"FPS: {fps:F0}\n";

        // -----------------------------------------------------------------
        // RESOURCE MANAGER STATS (if available)
        // We check if ResourceManager.Instance exists because the
        // MemoryMonitor might be used in a scene without a ResourceManager.
        // -----------------------------------------------------------------
        if (ResourceManager.Instance != null)
        {
            info += $"\n--- Resource Cache ---\n";
            info += $"Cached Textures: {ResourceManager.Instance.GetCacheCount()}\n";

            // Convert the resource manager's memory tracking to MB
            float texMemMB = ResourceManager.Instance.GetTotalMemory() / 1024f / 1024f;
            info += $"Texture Memory: {texMemMB:F2} MB\n";
        }

        // -----------------------------------------------------------------
        // KEYBOARD CONTROLS REMINDER
        // -----------------------------------------------------------------
        info += $"\n--- Controls ---\n";
        info += "U = Unload texture\n";
        info += "C = Clear cache\n";
        info += "R = Reload all";

        // Set the text on the UI element
        displayText.text = info;
    }
}
