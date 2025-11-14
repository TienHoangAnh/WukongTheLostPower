using System.Collections.Generic;
using UnityEngine;

// simple in-editor database: create assets and assign here in inspector
public class InventoryDatabase : MonoBehaviour
{
 public static InventoryDatabase I { get; private set; }
 public List<InventoryItem> items = new List<InventoryItem>();

 void Awake()
 {
 if (I != null) { Destroy(gameObject); return; }
 I = this; DontDestroyOnLoad(gameObject);
 }

 public InventoryItem GetById(string id)
 {
 return items.Find(i => i != null && i.id == id);
 }

 public static InventoryItem GetByIdStatic(string id) => I != null ? I.GetById(id) : null;
 // convenience for UI
}
