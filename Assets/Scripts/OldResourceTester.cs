// ResourceTester.cs
using UnityEngine;

public class OldResourceTester : MonoBehaviour
{
    public string[] texturePaths = {
    "squareVar1",
    "squareVar2",
    "squareVar3",
    "squareVar4"
    };
    
    void Start()
    {
        Debug.Log("Starting Resource Manager Test...");
        Invoke("RunTest", 1f);
    }
    
    void RunTest()
    {
        // Load each texture 3 times
        for (int i = 0; i < 3; i++)
        {
        Debug.Log($"\n--- Load Pass {i + 1} ---");
        foreach (string path in texturePaths)
        {
        Texture2D tex = OldResourceManager.Instance.LoadTexture(path);
        }
        }
        // Print statistics
        Debug.Log("\n");
        OldResourceManager.Instance.OldPrintStats();
        // Expected result:
        // - 4 textures loaded (first pass)
        // - 8 cache hits (second and third passes)
    }

    void Update()
    {
        // Press U to unload a specific texture
        if (Input.GetKeyDown(KeyCode.U))
        {
            OldResourceManager.Instance.UnloadTexture(texturePaths[0]);
            OldResourceManager.Instance.OldPrintStats();
        }
        
        // Press C to clear all cache
        if (Input.GetKeyDown(KeyCode.C))
        {
            OldResourceManager.Instance.ClearCache();
            OldResourceManager.Instance.OldPrintStats();
        }
    }
}