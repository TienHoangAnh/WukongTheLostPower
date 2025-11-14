using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager I { get; private set; }

    // store counts by id
    private Dictionary<string, int> items = new Dictionary<string, int>();

    // event to notify UI or other systems when inventory changes
    public event Action OnInventoryChanged;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        // load from GameSaveController if exists
        if (GameSaveController.I != null)
        {
            foreach (var kv in GameSaveController.I.CollectedCounts)
                items[kv.Key] = kv.Value;
        }
    }

    public void AddItem(string id, int count =1)
    {
        if (string.IsNullOrEmpty(id) || count <=0) return;
        if (!items.ContainsKey(id)) items[id] =0;
        items[id] += count;

        // also update global save
        GameSaveController.I?.MarkCollected(id, count);

        UI_Toasts.Show($"+{count} {id}");

        OnInventoryChanged?.Invoke();
    }

    public bool UseItem(string id, int count =1)
    {
        if (string.IsNullOrEmpty(id) || count <=0) return false;
        if (!items.ContainsKey(id) || items[id] < count) return false;

        items[id] -= count;
        if (items[id] <=0) items.Remove(id);

        GameSaveController.I?.UseItem(id, count);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public int GetCount(string id) => items.ContainsKey(id) ? items[id] :0;

    public Dictionary<string, int> GetAll() => items;
}
