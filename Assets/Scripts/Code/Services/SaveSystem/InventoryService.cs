using UnityEngine;
using System.Collections.Generic;

public class InventoryService : MonoBehaviour
{
    public static InventoryService Instance;
    private Dictionary<string, int> items = new();
    private HashSet<string> shards = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(string id, int count = 1)
    {
        if (!items.ContainsKey(id)) items[id] = 0;
        items[id] += count;
        UI_Toasts.Show($"+{count} {id}");
    }

    public void AddShard(string shardId)
    {
        if (shards.Contains(shardId)) return;
        shards.Add(shardId);
        UI_Toasts.Show($"Received Shard {shardId}");
    }

    public int GetCount(string id) => items.ContainsKey(id) ? items[id] : 0;
    public bool HasShard(string id) => shards.Contains(id);

    public Dictionary<string, int> GetAllItems() => items;
    public HashSet<string> GetAllShards() => shards;
}
