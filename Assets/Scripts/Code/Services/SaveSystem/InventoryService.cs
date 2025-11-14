using System.Collections.Generic;
using UnityEngine;

namespace Game.SaveSystem
{
    public class InventoryService : MonoBehaviour
    {
        public static InventoryService Instance { get; private set; }
        private Dictionary<string, int> itemCounts = new();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void AddItem(string id, int amount = 1)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!itemCounts.ContainsKey(id)) itemCounts[id] = 0;
            itemCounts[id] += amount;
            Debug.Log($"[Inventory] Added {amount}x {id} (total {itemCounts[id]})");
        }

        public int GetCount(string id) =>
            itemCounts.ContainsKey(id) ? itemCounts[id] : 0;
    }
}
