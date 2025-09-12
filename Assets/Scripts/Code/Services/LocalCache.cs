using System.IO;
using UnityEngine;

public static class LocalCache
{
    static string PathFor(string slotId) =>
        Application.persistentDataPath + $"/save_{slotId}.json";

    public static void Write(string slotId, SaveSlotDTO data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(PathFor(slotId), json);
    }

    public static bool TryRead(string slotId, out SaveSlotDTO data)
    {
        var p = PathFor(slotId);
        if (File.Exists(p))
        {
            data = JsonUtility.FromJson<SaveSlotDTO>(File.ReadAllText(p));
            return true;
        }
        data = null; return false;
    }
}
