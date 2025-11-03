using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class FirebasePlayerService : MonoBehaviour
{
    public static FirebasePlayerService I { get; private set; }

    FirebaseAuth _auth;
    FirebaseFirestore _db;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> InitWhenReady()
    {
        if (_db != null) return true;

        _auth = FirebaseAuth.DefaultInstance;
        if (_auth.CurrentUser == null)
        {
            Debug.LogWarning("[Firebase] User not logged in yet");
            return false;
        }

        _db = FirebaseFirestore.DefaultInstance;
        return true;
    }

    string UID => _auth.CurrentUser.UserId;
    DocumentReference PlayerDoc() => _db.Collection("players").Document(UID);

    public async Task AddCollectedAsync(string id)
    {
        if (!await InitWhenReady()) return;

        await PlayerDoc().UpdateAsync(new Dictionary<string, object> {
            { "collectedIds", FieldValue.ArrayUnion(id) },
            { "lastSeen", FieldValue.ServerTimestamp }
        });
        Debug.Log($"[Firebase] Synced collected item: {id}");
    }

    public async Task SaveProfileAsync(string displayName, int level, int xp)
    {
        if (!await InitWhenReady()) return;

        await PlayerDoc().SetAsync(new Dictionary<string, object> {
            { "displayName", displayName },
            { "level", level },
            { "xp", xp },
            { "lastSeen", FieldValue.ServerTimestamp }
        }, SetOptions.MergeAll);
    }

    public async Task SaveAllCollectedAsync(IEnumerable<string> ids)
    {
        if (!await InitWhenReady()) return;

        await PlayerDoc().SetAsync(new Dictionary<string, object> {
            { "collectedIds", new List<string>(ids) },
            { "lastSeen", FieldValue.ServerTimestamp }
        }, SetOptions.MergeAll);
    }
}
