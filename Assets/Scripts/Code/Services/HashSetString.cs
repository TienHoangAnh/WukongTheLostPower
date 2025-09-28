using System.Collections.Generic;
#if FIREBASE_ENABLED
using Firebase.Firestore;
#endif

#if FIREBASE_ENABLED
[FirestoreData]
#endif
public class HashSetString
{
#if FIREBASE_ENABLED
    [FirestoreProperty]
#endif
    public List<string> items { get; set; } = new List<string>();

    public bool Contains(string id) => items != null && items.Contains(id);

    public void Add(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (items == null) items = new List<string>();
        if (!items.Contains(id)) items.Add(id);
    }

    public void Remove(string id)
    {
        if (items == null) return;
        if (items.Contains(id)) items.Remove(id);
    }
}
