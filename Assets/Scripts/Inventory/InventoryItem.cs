using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItem", menuName = "Inventory/Item", order =1)]
public class InventoryItem : ScriptableObject
{
 [Tooltip("Unique id used for saving and lookup. Should match CollectibleId Id when item is a world pickup.")]
 public string id;

 public string displayName = "Item";
 public Sprite icon;
 [TextArea] public string description;
 public bool stackable = true;
 public int maxStack =99;
}
