using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CollectibleId))]
public class CollectiblePickup : MonoBehaviour
{
    public static event Action<string, string> OnPicked;

    [HideInInspector] public string displayName;
    public bool destroyOnPickup = true;

    private CollectibleId _id;

    void Awake()
    {
        _id = GetComponent<CollectibleId>();

        // Lấy tên object trực tiếp luôn
        displayName = gameObject.name;

        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    async void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"[Pickup] Picked -> {displayName} ({_id.Id})");

        // Notify Player UI quick-slots
        PlayerUIManager.I?.AddItem(_id.Id, 1);

        // Lưu vào save data
        GameSaveController.I?.MarkCollected(_id.Id, 1);

        // Firebase sync
        if (FirebasePlayerService.I != null)
            await FirebasePlayerService.I.AddCollectedAsync(_id.Id);

        // Gửi event cho DebugHUD
        OnPicked?.Invoke(displayName, _id.Id);

        // Xử lý sau khi nhặt
        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
