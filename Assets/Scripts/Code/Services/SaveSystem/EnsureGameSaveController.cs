using UnityEngine;

[DefaultExecutionOrder(-100)]
public class EnsureGameSaveController : MonoBehaviour
{
    private const string PrefabPath = "Prefabs/GameSaveController"; // Resources/Prefabs/GameSaveController.prefab

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (GameSaveController.I != null) return;
        var prefab = Resources.Load<GameObject>(PrefabPath);
        if (prefab != null)
        {
            Object.Instantiate(prefab);
            Debug.Log("[Bootstrap] Instantiated GameSaveController prefab from Resources.");
        }
        else
        {
            Debug.LogWarning($"[Bootstrap] GameSaveController prefab not found at Resources/{PrefabPath}. Please create and place it there.");
        }
    }
}