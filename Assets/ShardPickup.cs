//using UnityEngine;

//public class ShardPickup : MonoBehaviour
//{
//    public string shardId; // "KIM","MOC","THUY","HOA","THO"
//    void OnTriggerEnter(Collider other)
//    {
//        if (!other.CompareTag("Player")) return;
//        InventoryService.Instance.AddShard(shardId);
//        SaveSystem.Save(GameSaveController.I.Data);
//        UI_Toasts.Show($"+ Mảnh {shardId}");
//        gameObject.SetActive(false);
//    }
//}
