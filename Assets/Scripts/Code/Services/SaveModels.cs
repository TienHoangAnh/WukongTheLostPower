using System.Collections.Generic;
using UnityEngine;

#if FIREBASE_ENABLED
using Firebase.Firestore;
#endif

// --------- Vector3 DTO (Firestore-friendly) ---------
#if FIREBASE_ENABLED
[FirestoreData]
#endif
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

// --------- Player State ---------
#if FIREBASE_ENABLED
[FirestoreData]
#endif
public class PlayerStateDTO
{
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public float hp { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public float stamina { get; set; }

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

// --------- Item Stack (nếu cần) ---------
#if FIREBASE_ENABLED
[FirestoreData]
#endif
public class ItemStackDTO
{
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public string itemId { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public int qty { get; set; }
}

// --------- Save Slot ---------
#if FIREBASE_ENABLED
[FirestoreData]
#endif
public class SaveSlotDTO
{
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
    public int chapterIndex { get; set; }

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public PlayerStateDTO player { get; set; } = new PlayerStateDTO();

#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public List<string> learnedSkills { get; set; } = new();

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
