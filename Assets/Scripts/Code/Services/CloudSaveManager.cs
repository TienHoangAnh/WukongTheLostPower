using System;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_ENABLED
using Firebase.Firestore;
#endif

///
/// cloud save manager with local-first strategy
/// 
/// Local-first save orchestrator (newest-wins via lastSavedAtUnix in ms)
public static class CloudSaveManager
{
    public static string CurrentSlotId = "slotA";

    public static async Task<SaveSlotDTO> TryLoadOrCreate(string slotId, string playerName)
    {
        CurrentSlotId = slotId;

        // 1) Local first
        SaveSlotDTO local = null;
        LocalCache.TryRead(slotId, out local);

#if FIREBASE_ENABLED
        SaveSlotDTO remote = null;
        try
        {
            await FirebaseRuntime.EnsureInitializedAsync();
            remote = await FirebaseLoad(slotId);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[CloudSaveManager] Firebase load failed, using local. {ex.Message}");
        }

        var resolved = Resolve(local, remote);
        if (resolved == null)
        {
            resolved = NewDefault(slotId, playerName);
            LocalCache.Write(slotId, resolved);
            _ = SafeFirebaseSave(slotId, resolved);
            return resolved;
        }

        LocalCache.Write(slotId, resolved);
        if (remote == null || resolved.lastSavedAtUnix > remote.lastSavedAtUnix)
            _ = SafeFirebaseSave(slotId, resolved);

        return resolved;
#else
        if (local != null) return local;
        var dto = NewDefault(slotId, playerName);
        LocalCache.Write(slotId, dto);
        return dto;
#endif
    }

    public static async Task SaveNow(SaveSlotDTO dto)
    {
        if (dto == null) return;

        TouchTimestamp(dto);
        LocalCache.Write(CurrentSlotId, dto);

#if FIREBASE_ENABLED
        await SafeFirebaseSave(CurrentSlotId, dto);
#endif
    }

    public static bool TryLoadLocal(out SaveSlotDTO dto) =>
        LocalCache.TryRead(CurrentSlotId, out dto);

    // ---------- helpers ----------

    private static void TouchTimestamp(SaveSlotDTO dto)
    {
        dto.lastSavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private static SaveSlotDTO Resolve(SaveSlotDTO local, SaveSlotDTO remote)
    {
        if (local == null && remote == null) return null;
        if (local == null) return remote;
        if (remote == null) return local;
        return (local.lastSavedAtUnix >= remote.lastSavedAtUnix) ? local : remote;
    }

    private static SaveSlotDTO NewDefault(string slotId, string playerName) => new SaveSlotDTO
    {
        // slotName phải là id của slot, không phải tên người chơi
        slotName = string.IsNullOrWhiteSpace(slotId) ? "slotA" : slotId,
        playerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim(),

        // ⚠ dùng model mới
        currentMap = 1,
        essencesCollected = 0,
        playTimeSeconds = 0f,

        player = new PlayerStateDTO
        {
            hp = 100,
            stamina = 100,
            flask = 3,
            pos = new Vector3DTO(Vector3.zero),
            rotY = 0
        },

        inventory = new InventorySnapshot
        {
            holy_water = 0,
            elixir = 0,
            power_pill = 0
        },

        skillsUnlocked = new System.Collections.Generic.List<string>(),
        bossesDefeated = new System.Collections.Generic.List<string>(),
        deadEnemies = new System.Collections.Generic.List<string>(),
        worldFlags = new System.Collections.Generic.Dictionary<string, bool>(),
        skillCooldowns = new System.Collections.Generic.Dictionary<string, float>(),

        lastSavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        version = 1
    };

#if FIREBASE_ENABLED
    private static async Task<SaveSlotDTO> FirebaseLoad(string slotId)
    {
        var db = FirebaseRuntime.Db;
        var uid = FirebaseRuntime.Auth.CurrentUser.UserId;

        var doc = db.Collection("users").Document(uid)
                    .Collection("saveSlots").Document(slotId);

        var snap = await doc.GetSnapshotAsync();
        if (!snap.Exists) return null;
        return snap.ConvertTo<SaveSlotDTO>();
    }

    private static async Task FirebaseSave(string slotId, SaveSlotDTO data)
    {
        var db = FirebaseRuntime.Db;
        var uid = FirebaseRuntime.Auth.CurrentUser.UserId;

        var doc = db.Collection("users").Document(uid)
                    .Collection("saveSlots").Document(slotId);

        await doc.SetAsync(data, SetOptions.MergeAll);
    }

    private static async Task SafeFirebaseSave(string slotId, SaveSlotDTO data)
    {
        try
        {
            await FirebaseRuntime.EnsureInitializedAsync();
            await FirebaseSave(slotId, data);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[CloudSaveManager] Firebase save failed, kept local. {ex.Message}");
        }
    }
#endif
}
