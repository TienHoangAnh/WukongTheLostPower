using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_ENABLED
using Firebase.Firestore;
#endif

// Vector3 DTO
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[System.Serializable]
#if FIREBASE_ENABLED
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
#else
[System.Serializable]
public class Vector3DTO
{
 public float x;
 public float y;
 public float z;

 public Vector3DTO() { }
 public Vector3DTO(Vector3 v) { x = v.x; y = v.y; z = v.z; }
 public Vector3 ToVector3() => new Vector3(x, y, z);
}
#endif

// PlayerStateDTO
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[System.Serializable]
#if FIREBASE_ENABLED
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
#else
[System.Serializable]
public class PlayerStateDTO
{
 public int hp;
 public int stamina;
 public float rotY;
 public Vector3DTO pos = new Vector3DTO();
}
#endif

// InventorySnapshot
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[System.Serializable]
#if FIREBASE_ENABLED
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

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public int power_pill { get; set; } =0;
    }
#else
[System.Serializable]
public class InventorySnapshot
{
 public int holy_water;
 public int elixir;
 public int natural_energy;
 public int power_pill =0;
}
#endif

// SaveSlotDTO
#if FIREBASE_ENABLED
[FirestoreData]
#endif
[System.Serializable]
#if FIREBASE_ENABLED
public class SaveSlotDTO
    {
    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public int deathCount { get; set; } =0;

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
     public int currentMap { get; set; } =1;

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public int chapterIndex { get; set; } =1;

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public float playTimeSeconds { get; set; } =0f;

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public PlayerStateDTO player { get; set; } = new PlayerStateDTO();

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public InventorySnapshot inventory { get; set; } = new InventorySnapshot();

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public List<string> skillsUnlocked { get; set; } = new List<string>();

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public int essencesCollected { get; set; } =0;

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public Dictionary<string, int> collectedCounts { get; set; } = new Dictionary<string, int>();

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public List<string> bossesDefeated { get; set; } = new List<string>();

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public List<string> deadEnemies { get; set; } = new List<string>();

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public Dictionary<string, bool> worldFlags { get; set; } = new Dictionary<string, bool>();

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public Dictionary<string, float> skillCooldowns { get; set; } = new Dictionary<string, float>();

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public long lastSavedAtUnix { get; set; }

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public int version { get; set; } =1;

     // Player behavior tracking
    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public int meleeCount { get; set; } =0;

    #if FIREBASE_ENABLED
     [FirestoreProperty]
    #endif
     public int rangedCount { get; set; } =0;
    }
#else
[System.Serializable]
    public class SaveSlotDTO
    {
     public int deathCount =0;
     public string slotName;
     public string playerName;
     public int currentMap =1;
     public int chapterIndex =1;
     public float playTimeSeconds =0f;
     public PlayerStateDTO player = new PlayerStateDTO();
     public InventorySnapshot inventory = new InventorySnapshot();
     public List<string> skillsUnlocked = new List<string>();
     public int essencesCollected =0;
     public Dictionary<string, int> collectedCounts = new Dictionary<string, int>();
     public List<string> bossesDefeated = new List<string>();
     public List<string> deadEnemies = new List<string>();
     public Dictionary<string, bool> worldFlags = new Dictionary<string, bool>();
     public Dictionary<string, float> skillCooldowns = new Dictionary<string, float>();
     public long lastSavedAtUnix;
     public int version =1;

     // Player behavior tracking
     public int meleeCount =0;
     public int rangedCount =0;
    }
#endif
