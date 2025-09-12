using Firebase.Firestore;
using System;
using System.Threading.Tasks;
using FSource = Firebase.Firestore.Source; // 👈 alias

public class FirebaseSaveService
{
    readonly FirebaseFirestore _db;
    readonly string _uid;

    public FirebaseSaveService(string uid)
    {
        _db = FirebaseFirestore.DefaultInstance;
        _uid = uid;
    }

    DocumentReference SlotRef(string slotId) =>
        _db.Collection("users").Document(_uid)
           .Collection("saveSlots").Document(slotId);

    public async Task<SaveSlotDTO> LoadAsync(string slotId)
    {
        var snap = await SlotRef(slotId).GetSnapshotAsync(FSource.Default);  // 👈 dùng alias
        if (!snap.Exists) return null;
        return snap.ConvertTo<SaveSlotDTO>();
    }

    public async Task SaveAsync(string slotId, SaveSlotDTO data)
    {
        data.lastSavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await SlotRef(slotId).SetAsync(data, SetOptions.MergeAll);          // OK
        // hoặc fully-qualify:
        // await SlotRef(slotId).SetAsync(data, Firebase.Firestore.SetOptions.MergeAll);
    }

    public async Task<bool> ExistsAsync(string slotId)
    {
        var s = await SlotRef(slotId).GetSnapshotAsync(FSource.Default);    // 👈 dùng alias
        return s.Exists;
    }
}
