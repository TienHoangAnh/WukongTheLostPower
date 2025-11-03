using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public static ChapterManager Instance { get; private set; }

    [Header("Chapter Settings")]
    [Tooltip("Start Map")]
    public int currentMap = 1;

    [Tooltip("Total map of game")]
    public int maxMap = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetNextMapName()
    {
        return $"Map{currentMap}";
    }

    public bool HasNextMap()
    {
        return currentMap < maxMap;
    }

    public void AdvanceMap()
    {
        if (HasNextMap())
        {
            currentMap++;
            Debug.Log($"[ChapterManager] Moved to Map {currentMap}");
        }
        else
        {
            Debug.Log("[ChapterManager] Already at final map!");
        }
    }

    public void ResetToFirst()
    {
        currentMap = 1;
        Debug.Log("[ChapterManager] Reset to Map1");
    }
}
