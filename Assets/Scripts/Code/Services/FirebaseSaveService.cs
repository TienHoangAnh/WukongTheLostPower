using System;
using System.Threading.Tasks;
using Firebase.Firestore;

/// <summary>
/// Service for saving and loading SaveSlotDTO objects to and from Firestore for a given user.
/// </summary>
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

    /// <summary>
    /// Returns a document reference for a specific save slot under the user's collection.
    /// </summary>
    private DocumentReference SlotRef(string slotId) =>
        _db.Collection("users").Document(_uid)
           .Collection("saveSlots").Document(slotId);

    /// <summary>
    /// Loads a save slot from Firestore. Returns null if the document does not exist.
    /// </summary>
    public async Task<SaveSlotDTO> LoadAsync(string slotId)
    {
        var snap = await SlotRef(slotId).GetSnapshotAsync(DefaultSource);
        if (!snap.Exists)
            return null;

        return snap.ConvertTo<SaveSlotDTO>();
    }

    /// <summary>
    /// Saves the given SaveSlotDTO to Firestore using MergeAll semantics,
    /// updating lastSavedAtUnix to the current time before persisting.
    /// </summary>
    public async Task SaveAsync(string slotId, SaveSlotDTO data)
    {
        data.lastSavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await SlotRef(slotId).SetAsync(data, SetOptions.MergeAll);
    }

    /// <summary>
    /// Checks whether a save slot document exists in Firestore.
    /// </summary>
    public async Task<bool> ExistsAsync(string slotId)
    {
        var snap = await SlotRef(slotId).GetSnapshotAsync(DefaultSource);
        return snap.Exists;
    }
}
