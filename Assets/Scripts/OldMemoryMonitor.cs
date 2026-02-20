// MemoryMonitor.cs - Attach to UI Text component
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OldMemoryMonitor : MonoBehaviour
{
    public TextMeshProUGUI displayText;
    public float updateInterval = 0.5f;
    private float timer;
    
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
        UpdateDisplay();
        timer = 0;
        }
    }

    void UpdateDisplay()
    {
        // Get total allocated memory
        long totalMemory = System.GC.GetTotalMemory(false);
        float memoryMB = totalMemory / 1024f / 1024f;
        // Build display string
        string info = $"Memory Monitor\n";
        info += $"Total: {memoryMB:F2} MB\n";
        info += $"FPS: {1f / Time.deltaTime:F0}\n\n";
        info += "Press U to unload texture\n";
        info += "Press C to clear cache";
        displayText.text = info;
    }
}