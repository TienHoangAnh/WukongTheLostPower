using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class LocalCache
{
    static string PathFor(string slotId) =>
        Application.persistentDataPath + $"/save_{slotId}.json";

    public static void Write(string slotId, SaveSlotDTO data)
    {
        try
        {
            SaveSlotDTO toWrite = data ?? new SaveSlotDTO();

            // Convert to a simple serializable object that uses public fields so JsonUtility can serialize
            var simple = ConvertToSimple(toWrite);

            var json = JsonUtility.ToJson(simple, true);
            var path = PathFor(slotId);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            Debug.Log($"[LocalCache] Wrote save for slot '{slotId}' to: {path}\n{json}");
#else
            Debug.Log($"[LocalCache] Wrote save for slot '{slotId}' to: {path}");
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LocalCache] Failed to write save for slot '{slotId}': {ex.Message}");
        }
    }

    public static bool TryRead(string slotId, out SaveSlotDTO data)
    {
        var p = PathFor(slotId);
        data = null;
        try
        {
            if (File.Exists(p))
            {
                var content = File.ReadAllText(p);
                if (string.IsNullOrWhiteSpace(content) || content.Trim() == "{}")
                {
                    // Empty or placeholder file - treat as missing
                    Debug.LogWarning($"[LocalCache] Found empty or placeholder save file for slot '{slotId}' at {p}");
                    return false;
                }

                // Try to deserialize into the simple shape first
                var simple = JsonUtility.FromJson<SimpleSaveSlot>(content);
                if (simple == null)
                {
                    Debug.LogWarning($"[LocalCache] Failed to parse simple save format from {p}");
                    return false;
                }

                // Convert back to SaveSlotDTO
                data = ConvertFromSimple(simple);
                return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[LocalCache] Failed to read save for slot '{slotId}': {ex.Message}");
        }
        return false;
    }

    // Convert property/field-based SaveSlotDTO into a simple serializable form
    private static SimpleSaveSlot ConvertToSimple(SaveSlotDTO d)
    {
        var s = new SimpleSaveSlot();
        s.deathCount = d.deathCount;
        s.slotName = d.slotName;
        s.playerName = d.playerName;
        s.currentMap = d.currentMap;
        s.chapterIndex = d.chapterIndex;
        s.playTimeSeconds = d.playTimeSeconds;

        s.player = new SimplePlayerState();
        if (d.player != null)
        {
            s.player.hp = d.player.hp;
            s.player.stamina = d.player.stamina;
            s.player.rotY = d.player.rotY;
            if (d.player.pos != null)
            {
                s.player.pos = new float[3] { d.player.pos.x, d.player.pos.y, d.player.pos.z };
            }
        }

        s.inventory = new SimpleInventory();
        if (d.inventory != null)
        {
            s.inventory.holy_water = d.inventory.holy_water;
            s.inventory.elixir = d.inventory.elixir;
            s.inventory.natural_energy = d.inventory.natural_energy;
            s.inventory.power_pill = d.inventory.power_pill;
        }

        s.skillsUnlocked = (d.skillsUnlocked != null) ? d.skillsUnlocked.ToArray() : new string[0];

        // convert collectedCounts dictionary to array
        if (d.collectedCounts != null)
        {
            var list = new List<SimpleKeyValue>();
            foreach (var kv in d.collectedCounts)
                list.Add(new SimpleKeyValue { key = kv.Key, value = kv.Value });
            s.collectedCounts = list.ToArray();
        }
        else s.collectedCounts = new SimpleKeyValue[0];

        s.lastSavedAtUnix = d.lastSavedAtUnix;
        s.version = d.version;

        // copy new behavior counters
        s.meleeCount = d.meleeCount;
        s.rangedCount = d.rangedCount;

        return s;
    }

    private static SaveSlotDTO ConvertFromSimple(SimpleSaveSlot s)
    {
        var d = new SaveSlotDTO();
        d.deathCount = s.deathCount;
        d.slotName = s.slotName;
        d.playerName = s.playerName;
        d.currentMap = s.currentMap;
        d.chapterIndex = s.chapterIndex;
        d.playTimeSeconds = s.playTimeSeconds;

        if (d.player == null) d.player = new PlayerStateDTO();
        if (s.player != null)
        {
            d.player.hp = s.player.hp;
            d.player.stamina = s.player.stamina;
            d.player.rotY = s.player.rotY;
            if (s.player.pos != null && s.player.pos.Length >= 3)
            {
                d.player.pos = new Vector3DTO(new UnityEngine.Vector3(s.player.pos[0], s.player.pos[1], s.player.pos[2]));
            }
        }

        if (d.inventory == null) d.inventory = new InventorySnapshot();
        if (s.inventory != null)
        {
            d.inventory.holy_water = s.inventory.holy_water;
            d.inventory.elixir = s.inventory.elixir;
            d.inventory.natural_energy = s.inventory.natural_energy;
            d.inventory.power_pill = s.inventory.power_pill;
        }

        d.skillsUnlocked = new List<string>();
        if (s.skillsUnlocked != null)
            d.skillsUnlocked.AddRange(s.skillsUnlocked);

        if (s.collectedCounts != null)
        {
            d.collectedCounts = new Dictionary<string, int>();
            foreach (var kv in s.collectedCounts)
                d.collectedCounts[kv.key] = kv.value;
        }

        d.lastSavedAtUnix = s.lastSavedAtUnix;
        d.version = s.version;

        // restore behavior counters
        d.meleeCount = s.meleeCount;
        d.rangedCount = s.rangedCount;

        return d;
    }

    // Simple serializable classes
    [System.Serializable]
    public class SimpleSaveSlot
    {
        public int deathCount;
        public string slotName;
        public string playerName;
        public int currentMap;
        public int chapterIndex;
        public float playTimeSeconds;
        public SimplePlayerState player;
        public SimpleInventory inventory;
        public string[] skillsUnlocked;
        public SimpleKeyValue[] collectedCounts;
        public long lastSavedAtUnix;
        public int version;

        // Player behavior counters
        public int meleeCount;
        public int rangedCount;
    }

    [System.Serializable]
    public class SimplePlayerState
    {
        public int hp;
        public int stamina;
        public float rotY;
        public float[] pos; // x,y,z
    }

    [System.Serializable]
    public class SimpleInventory
    {
        public int holy_water;
        public int elixir;
        public int natural_energy;
        public int power_pill;
    }

    [System.Serializable]
    public class SimpleKeyValue
    {
        public string key;
        public int value;
    }
}
