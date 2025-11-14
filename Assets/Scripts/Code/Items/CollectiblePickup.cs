using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CollectibleId))]
public class CollectiblePickup : MonoBehaviour
{
    public static event Action<string, string> OnPicked; // (displayName, id)

    public string displayName = "Item";
    public bool destroyOnPickup = true;

    private CollectibleId _id;

    void Awake()
    {
        _id = GetComponent<CollectibleId>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    async void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"[Pickup] Picked -> {displayName} ({_id.Id})");

        // lưu vào save data (tăng số lượng)
        GameSaveController.I?.MarkCollected(_id.Id, 1);

        // nếu có Firebase thì đồng bộ lên cloud
        if (FirebasePlayerService.I != null)
            await FirebasePlayerService.I.AddCollectedAsync(_id.Id);

        // phát sự kiện cho HUD / hệ thống UI
        OnPicked?.Invoke(displayName, _id.Id);

        // xử lý sau khi nhặt
        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
