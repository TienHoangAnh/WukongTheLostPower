using UnityEngine;

/// <summary>
/// UIBootstrap ensures a PlayerUIManager exists when the game begins.
/// - If scene already contains a HUD (PlayerUIManager), it makes it persistent.
/// - If not, it loads HUD prefab from Resources.
/// - Does NOT overwrite PlayerUIManager.I because setter is private.
/// </summary>

[DefaultExecutionOrder(-1000)]
public class UIBootstrap : MonoBehaviour
{
    // Prefab path under Resources/ (e.g. Resources/HUD.prefab)
    public string hudResourcePath = "HUD";

    void Awake()
    {
        // CASE 1: PlayerUIManager already set itself in a previous scene
        if (PlayerUIManager.I != null)
        {
            Debug.Log("[UIBootstrap] PlayerUIManager already present (singleton set).");
            return;
        }

        // CASE 2: Check if this scene has a PlayerUIManager component somewhere
        var existing = FindFirstObjectByType<PlayerUIManager>();
        if (existing != null)
        {
            Debug.Log("[UIBootstrap] Found PlayerUIManager in scene, making it persistent.");

            // DO NOT assign I — PlayerUIManager handles that itself in Awake()
            DontDestroyOnLoad(existing.gameObject);

            return;
        }

        // CASE 3: No UI found — try load HUD prefab from Resources
        var prefab = Resources.Load<GameObject>(hudResourcePath);

        if (prefab != null)
        {
            var inst = Instantiate(prefab);
            inst.name = prefab.name;

            DontDestroyOnLoad(inst);

            Debug.Log("[UIBootstrap] Instantiated HUD prefab from Resources and made persistent.");
            return;
        }

        // FAIL CASE
        Debug.LogWarning($"[UIBootstrap] No HUD found and no prefab at Resources/{hudResourcePath}. Please add HUD prefab.");
    }
}
