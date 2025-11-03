using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveController : MonoBehaviour
{
    public static GameSaveController I { get; private set; }

    public SaveData Data { get; private set; }

    public HashSet<string> CollectedIds { get; private set; }

    public Dictionary<string, int> CollectedCounts => Data.collectedCounts;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        Data = SaveSystem.Load();

        if (Data.collectedCounts == null)
            Data.collectedCounts = new Dictionary<string, int>();

        CollectedIds = new HashSet<string>(Data.collectedIds ?? new List<string>());
        Debug.Log($"[GameSave] Loaded. Collected = {CollectedIds.Count} | WithCounts = {Data.collectedCounts.Count}");
    }

    public bool IsCollected(string id) => CollectedIds.Contains(id);


    public void MarkCollected(string id, int amount = 1)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0) return;

        if (!Data.collectedCounts.ContainsKey(id))
            Data.collectedCounts[id] = 0;

        Data.collectedCounts[id] += amount;

        if (CollectedIds.Add(id))
            Data.collectedIds.Add(id);

        SaveSystem.Save(Data);

        Debug.Log($"[GameSave] Collected {id} x{amount} (Total: {Data.collectedCounts[id]})");

        try { FirebasePlayerService.I?.AddCollectedAsync(id); } catch { }
    }

    public int GetCollectedCount(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;
        return Data.collectedCounts.TryGetValue(id, out var count) ? count : 0;
    }

    public void WipeAndReload()
    {
        System.IO.File.Delete(System.IO.Path.Combine(Application.persistentDataPath, "save.json"));
        Data = new SaveData();
        CollectedIds.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
