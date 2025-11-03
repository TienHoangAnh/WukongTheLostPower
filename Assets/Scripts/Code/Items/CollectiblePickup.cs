using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CollectibleId))]
public class CollectiblePickup : MonoBehaviour
{
    public string displayName = "Item";
    public bool destroyOnPickup = true;

    CollectibleId _id;

    void Awake()
    {
        _id = GetComponent<CollectibleId>();
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Start()
    {
        if (GameSaveController.I != null && GameSaveController.I.IsCollected(_id.Id))
        {
            Debug.Log($"[Pickup] Already collected -> {displayName} ({_id.Id}). Hide");
            gameObject.SetActive(false);
        }
    }

    async void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameSaveController.I == null) { Debug.LogWarning("[Pickup] Missing GameSaveController"); return; }

        Debug.Log($"[Pickup] Picked -> {displayName} ({_id.Id})");

        GameSaveController.I.MarkCollected(_id.Id);           // JSON local
        if (FirebasePlayerService.I != null)                  // Cloud (optional)
            await FirebasePlayerService.I.AddCollectedAsync(_id.Id);

        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
