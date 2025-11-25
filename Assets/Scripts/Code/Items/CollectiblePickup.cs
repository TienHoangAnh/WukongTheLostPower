using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CollectibleId))]
public class CollectiblePickup : MonoBehaviour
{
    /// <summary>
    /// Global event raised when an item is picked up.
    /// Parameters: (displayName, itemKey).
    /// </summary>
    public static event Action<string, string> OnPicked;

    [HideInInspector]
    public string displayName;

    [Tooltip("If true, the GameObject will be destroyed after pickup. Otherwise it will be deactivated.")]
    public bool destroyOnPickup = true;

    private CollectibleId _id;

    private void Awake()
    {
        // Cache collectible identifier
        _id = GetComponent<CollectibleId>();

        // Default display name to the GameObject name
        displayName = gameObject.name;

        // Ensure collider works as a trigger volume
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private async void OnTriggerEnter(Collider other)
    {
        // Only react to the player
        if (!other.CompareTag("Player"))
            return;

        Debug.Log($"[Pickup] Picked -> {displayName} ({_id.Key})");

        // Update player UI / inventory representation
        PlayerUIManager.I?.AddItem(_id.Key, 1);

        // Update local save data
        GameSaveController.I?.MarkCollected(_id.Key, 1);

        // Sync to cloud if available
        if (FirebasePlayerService.I != null)
            await FirebasePlayerService.I.AddCollectedAsync(_id.Key);

        // Notify listeners (e.g. debug HUD, shard listeners, analytics)
        OnPicked?.Invoke(displayName, _id.Key);

        // Remove or hide the world object after pickup
        if (destroyOnPickup)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
