using System;
using System.Threading.Tasks;
using Firebase.Firestore;

public class FirebaseSaveService
{
    private readonly FirebaseFirestore _db;
    private readonly string _uid;

    private static readonly Firebase.Firestore.Source DefaultSource = Firebase.Firestore.Source.Default;

    public FirebaseSaveService(string uid)
    {
        _db = FirebaseFirestore.DefaultInstance;
        _uid = uid;
    }

    private DocumentReference SlotRef(string slotId) =>
        _db.Collection("users").Document(_uid)
           .Collection("saveSlots").Document(slotId);

    public async Task<SaveSlotDTO> LoadAsync(string slotId)
    {
        var snap = await SlotRef(slotId).GetSnapshotAsync(DefaultSource);
        if (!snap.Exists)
            return null;

        return snap.ConvertTo<SaveSlotDTO>();
    }

    public async Task SaveAsync(string slotId, SaveSlotDTO data)
    {
        data.lastSavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await SlotRef(slotId).SetAsync(data, SetOptions.MergeAll);
    }

    public async Task<bool> ExistsAsync(string slotId)
    {
        var snap = await SlotRef(slotId).GetSnapshotAsync(DefaultSource);
        return snap.Exists;
    }
}
