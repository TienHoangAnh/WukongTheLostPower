using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public static ChapterManager Instance { get; private set; }

    [Header("Chapter Settings")]
    [Tooltip("Start Map")]
    public int currentMap = 1;

    [Tooltip("Total map of game")]
    public int maxMap = 6;

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

            // Persist chapter/map into runtime save so SaveSlotDTO reflects current progress
            try
            {
                SaveRuntime.EnsureInitialized();
                if (SaveRuntime.Current != null)
                {
                    SaveRuntime.Current.currentMap = currentMap;
                    SaveRuntime.Current.chapterIndex = currentMap;
                    // Fire-and-forget cloud/local save to persist the change
                    _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ChapterManager] Failed to persist chapter change: {ex.Message}");
            }
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

        try
        {
            SaveRuntime.EnsureInitialized();
            if (SaveRuntime.Current != null)
            {
                SaveRuntime.Current.currentMap = currentMap;
                SaveRuntime.Current.chapterIndex = currentMap;
                _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ChapterManager] Failed to persist chapter reset: {ex.Message}");
        }
    }
}
