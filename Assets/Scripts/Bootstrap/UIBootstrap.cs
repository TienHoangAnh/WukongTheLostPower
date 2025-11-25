using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class UIBootstrap : MonoBehaviour
{
 // Name of prefab placed under Resources (e.g. Assets/Resources/HUD.prefab)
 public string hudResourcePath = "HUD";

 void Awake()
 {
 // If a persistent PlayerUIManager already exists, nothing to do
 if (PlayerUIManager.I != null)
 {
 Debug.Log("[UIBootstrap] PlayerUIManager already present.");
 return;
 }

 // Try to find a UI in the current scene
 var existing = FindFirstObjectByType<PlayerUIManager>();
 if (existing != null)
 {
 Debug.Log("[UIBootstrap] Found PlayerUIManager in scene, making persistent.");
 DontDestroyOnLoad(existing.gameObject);
 PlayerUIManager.I = existing; // in case Awake ordering prevented it
 return;
 }

 // Otherwise try to load HUD prefab from Resources
 var prefab = Resources.Load<GameObject>(hudResourcePath);
 if (prefab != null)
 {
 var inst = Instantiate(prefab);
 inst.name = prefab.name;
 DontDestroyOnLoad(inst);
 Debug.Log("[UIBootstrap] Instantiated HUD prefab and made persistent.");
 return;
 }

 Debug.LogWarning($"[UIBootstrap] No PlayerUIManager found and no HUD prefab at Resources/{hudResourcePath}. Please add a persistent HUD prefab or include UI in the bootstrap scene.");
 }
}
