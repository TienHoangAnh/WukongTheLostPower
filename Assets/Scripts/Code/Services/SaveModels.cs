using System;
using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_ENABLED
using Firebase.Firestore;
#endif

// ========== Vector3 DTO ==========
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[Serializable] // để Newtonsoft hoặc JsonUtility đều chấp nhận
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

    public Vector3DTO() { }
    public Vector3DTO(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

// ========== Player State ==========
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[Serializable]
public class PlayerStateDTO
{
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int hp { get; set; }          // int cho gameplay
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
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int flask { get; set; }
}

// ========== Inventory ==========
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
    public int power_pill { get; set; }
}

// ========== Save Slot ==========
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

    // Thống nhất tên "currentMap" để map trực tiếp gameplay
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int currentMap { get; set; } = 1;

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int essencesCollected { get; set; } = 0;

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

    // Thống nhất "skillsUnlocked"
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public List<string> skillsUnlocked { get; set; } = new();

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public List<string> bossesDefeated { get; set; } = new();

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public List<string> deadEnemies { get; set; } = new();

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public Dictionary<string, bool> worldFlags { get; set; } = new();

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public Dictionary<string, float> skillCooldowns { get; set; } = new();

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public long lastSavedAtUnix { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int version { get; set; } = 1;
}
