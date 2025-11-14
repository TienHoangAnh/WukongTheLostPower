using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryUI : MonoBehaviour
{
 public Transform contentParent; // parent transform to spawn item rows
 public GameObject itemRowPrefab; // prefab with Image + Text
 public KeyCode toggleKey = KeyCode.I;

 private CanvasGroup cg;
 private bool visible = false;

 void Awake()
 {
 cg = GetComponent<CanvasGroup>();
 if (InventoryManager.I != null)
 InventoryManager.I.OnInventoryChanged += Refresh;
 }

 void Start()
 {
 Hide();
 Refresh();
 }

 void OnDestroy()
 {
 if (InventoryManager.I != null)
 InventoryManager.I.OnInventoryChanged -= Refresh;
 }

 void Update()
 {
 if (Input.GetKeyDown(toggleKey))
 {
 visible = !visible;
 if (visible) Show(); else Hide();
 }
 }

 public void Show()
 {
 cg.alpha =1; cg.interactable = true; cg.blocksRaycasts = true;
 visible = true;
 Refresh();
 }

 public void Hide()
 {
 cg.alpha =0; cg.interactable = false; cg.blocksRaycasts = false;
 visible = false;
 }

 public void Refresh()
 {
 if (contentParent == null || itemRowPrefab == null || InventoryManager.I == null) return;

 // clear
 for (int i = contentParent.childCount -1; i >=0; i--) Destroy(contentParent.GetChild(i).gameObject);

 var items = InventoryManager.I.GetAll();
 foreach (var kv in items)
 {
 var go = Instantiate(itemRowPrefab, contentParent);
 var img = go.transform.Find("Icon")?.GetComponent<Image>();
 var txt = go.transform.Find("Label")?.GetComponent<Text>();
 if (img != null)
 {
 // try to find InventoryItem asset by id to get icon
 var asset = InventoryDatabase.I != null ? InventoryDatabase.I.GetById(kv.Key) : null;
 if (asset != null) img.sprite = asset.icon;
 }
 if (txt != null)
 {
 var asset = InventoryDatabase.I != null ? InventoryDatabase.I.GetById(kv.Key) : null;
 var name = asset != null ? asset.displayName : kv.Key;
 txt.text = $"{name} x{kv.Value}";
 }
 }
 }
}
