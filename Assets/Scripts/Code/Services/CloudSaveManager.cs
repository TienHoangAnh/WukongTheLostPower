using System;
using System.Threading.Tasks;
using UnityEngine;

#if FIREBASE_ENABLED
using Firebase.Firestore;
#endif

public static class CloudSaveManager
{
    public static string CurrentSlotId = "slotA";

    public static async Task<SaveSlotDTO> TryLoadOrCreate(string slotId, string playerName)
    {
        CurrentSlotId = slotId;

#if FIREBASE_ENABLED
        try
        {
            await FirebaseRuntime.EnsureInitializedAsync();

            var dto = await FirebaseLoad(slotId);
            if (dto == null)
            {
                dto = NewDefault(playerName);
                await FirebaseSave(slotId, dto);
            }

            LocalCache.Write(slotId, dto); // backup local
            return dto;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[CloudSaveManager] Firebase failed → local fallback: {ex.Message}");
            if (LocalCache.TryRead(slotId, out var local)) return local;

            var dto = NewDefault(playerName);
            LocalCache.Write(slotId, dto);
            return dto;
        }
#else
        if (LocalCache.TryRead(slotId, out var local2)) return local2;
        var dto2 = NewDefault(playerName);
        LocalCache.Write(slotId, dto2);
        return dto2;
#endif
    }

    public static async Task SaveNow(SaveSlotDTO dto)
    {
        if (dto == null) return;
        dto.lastSavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // luôn ghi local
        LocalCache.Write(CurrentSlotId, dto);

#if FIREBASE_ENABLED
        try
        {
            await FirebaseRuntime.EnsureInitializedAsync();
            await FirebaseSave(CurrentSlotId, dto);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[CloudSaveManager] Firebase save failed, kept local. {ex.Message}");
        }
#endif
    }

    public static bool TryLoadLocal(out SaveSlotDTO dto) => LocalCache.TryRead(CurrentSlotId, out dto);

    private static SaveSlotDTO NewDefault(string playerName) => new SaveSlotDTO
    {
        slotName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName,
        playerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName,
        chapterIndex = 1,
        player = new PlayerStateDTO
        {
            hp = 100,
            stamina = 100,
            flask = 3,
            pos = new Vector3DTO(Vector3.zero),
            rotY = 0
        }
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
#endif
}
