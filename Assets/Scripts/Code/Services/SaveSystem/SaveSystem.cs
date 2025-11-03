using System.IO;
using UnityEngine;

public static class SaveSystem
{
    static string PathFile => Path.Combine(Application.persistentDataPath, "save.json");

    public static SaveData Load()
    {
        if (!File.Exists(PathFile)) return new SaveData();
        var json = File.ReadAllText(PathFile);
        return JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
    }

    public static void Save(SaveData data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(PathFile, json);
#if UNITY_EDITOR
        Debug.Log($"[SaveSystem] Saved: {PathFile}\n{json}");
#endif
    }
}
