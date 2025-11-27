using System;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_ENABLED
using Firebase.Firestore;
#endif

// ========================= Vector3 DTO =========================
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[Serializable]
public class Vector3DTO
{
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public float x { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public float y { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public float z { get; set; }

    /// <summary>
    /// Parameterless constructor required by serializers.
    /// </summary>
    public Vector3DTO() { }

    /// <summary>
    /// Creates a DTO from a Unity Vector3.
    /// </summary>
    public Vector3DTO(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    /// <summary>
    /// Converts this DTO back to a Unity Vector3.
    /// </summary>
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

// ========================= Player State =========================
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[Serializable]
public class PlayerStateDTO
{
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int hp { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int stamina { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public float rotY { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public Vector3DTO pos { get; set; } = new Vector3DTO();
}

// ========================= Inventory Snapshot =========================
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[Serializable]
public class InventorySnapshot
{
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int holy_water { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int elixir { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int natural_energy { get; set; }

    /// <summary>
    /// Legacy field for a deprecated item type, kept for compatibility.
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int power_pill { get; set; } = 0;
}

// ========================= Save Slot DTO =========================
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[Serializable]
public class SaveSlotDTO
{
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int deathCount { get; set; } = 0;

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public string slotName { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public string playerName { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int currentMap { get; set; } = 1;

    // Add chapterIndex field to match legacy Firestore documents
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int chapterIndex { get; set; } = 1;

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public float playTimeSeconds { get; set; } = 0f;

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public PlayerStateDTO player { get; set; } = new PlayerStateDTO();

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public InventorySnapshot inventory { get; set; } = new InventorySnapshot();

    /// <summary>
    /// List of skill ids that have been unlocked for this save slot.
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public List<string> skillsUnlocked { get; set; } = new List<string>();

    // -------- Legacy / compatibility fields --------

    /// <summary>
    /// Aggregate count of essences collected across the run.
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int essencesCollected { get; set; } = 0;

    /// <summary>
    /// Per-item collected counts (e.g. pickups, shards, consumables).
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public Dictionary<string, int> collectedCounts { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Ids of bosses that have been defeated.
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public List<string> bossesDefeated { get; set; } = new List<string>();

    /// <summary>
    /// Ids of enemies that should remain dead in the world.
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public List<string> deadEnemies { get; set; } = new List<string>();

    /// <summary>
    /// Arbitrary world flags used by gameplay logic (e.g. doors opened, events triggered).
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public Dictionary<string, bool> worldFlags { get; set; } = new Dictionary<string, bool>();

    /// <summary>
    /// Remaining cooldowns for skills, used to restore cooldown state after loading.
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public Dictionary<string, float> skillCooldowns { get; set; } = new Dictionary<string, float>();

    /// <summary>
    /// Unix timestamp of the last save operation.
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public long lastSavedAtUnix { get; set; }

    /// <summary>
    /// Version number of the save schema, used for migrations.
    /// </summary>
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int version { get; set; } = 1;
}
