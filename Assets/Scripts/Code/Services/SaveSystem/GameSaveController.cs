using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveController : MonoBehaviour
{
    public static GameSaveController I { get; private set; }

    public SaveData Data { get; private set; }

    // Set dùng cho “đã từng nhặt” (flag)
    public HashSet<string> CollectedIds { get; private set; }

    // Đếm số lượng mỗi item
    public Dictionary<string, int> CollectedCounts => Data.collectedCounts;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        Data = SaveSystem.Load();

        // ✅ đảm bảo 2 cấu trúc luôn có
        if (Data.collectedCounts == null)
            Data.collectedCounts = new Dictionary<string, int>();
        if (Data.collectedIds == null)
            Data.collectedIds = new List<string>();

        CollectedIds = new HashSet<string>(Data.collectedIds);
        Debug.Log($"[GameSave] Loaded. Collected = {CollectedIds.Count} | WithCounts = {Data.collectedCounts.Count}");
    }

    // Nếu bạn muốn "đã nhặt" = có count > 0, có thể đổi thành: GetCollectedCount(id) > 0
    public bool IsCollected(string id) => CollectedIds.Contains(id);

    public void MarkCollected(string id, int amount = 1)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0) return;

        if (!Data.collectedCounts.ContainsKey(id))
            Data.collectedCounts[id] = 0;
        Data.collectedCounts[id] += amount;

        if (CollectedIds.Add(id)) // lần đầu thấy id này
            Data.collectedIds.Add(id);

        SaveSystem.Save(Data);

        Debug.Log($"[GameSave] Collected {id} x{amount} (Total: {Data.collectedCounts[id]})");

        // Nếu muốn sync cloud, có thể truyền cả count:
        // _ = FirebasePlayerService.I?.AddCollectedAsync(id, amount);
        try { FirebasePlayerService.I?.AddCollectedAsync(id); } catch { }
    }

    public int GetCollectedCount(string id) =>
        string.IsNullOrEmpty(id) ? 0 :
        (Data.collectedCounts.TryGetValue(id, out var c) ? c : 0);

    public void UseItem(string id, int amount = 1)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!Data.collectedCounts.ContainsKey(id)) return;

        Data.collectedCounts[id] = Mathf.Max(0, Data.collectedCounts[id] - amount);
        SaveSystem.Save(Data);
    }

    public void WipeAndReload()
    {
        System.IO.File.Delete(System.IO.Path.Combine(Application.persistentDataPath, "save.json"));
        Data = new SaveData { collectedCounts = new Dictionary<string, int>(), collectedIds = new List<string>() };
        CollectedIds.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
