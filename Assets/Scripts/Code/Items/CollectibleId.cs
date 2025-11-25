using UnityEngine;
using System;

[DisallowMultipleComponent]
public class CollectibleId : MonoBehaviour
{
    [SerializeField] private string uniqueId;

    [Tooltip("Optional logical item key (e.g. 'Shard_Metal'). If empty, the GUID uniqueId will be used.")]
    [SerializeField] private string itemId;

    /// <summary>
    /// Raw GUID value for this collectible instance.
    /// </summary>
    public string Id => uniqueId;

    /// <summary>
    /// Logical key used by gameplay systems. Uses itemId if set, otherwise falls back to uniqueId.
    /// </summary>
    public string Key => string.IsNullOrEmpty(itemId) ? uniqueId : itemId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure a GUID is generated if missing
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }

        // Provide a default itemId based on GameObject name if none is provided
        if (string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(gameObject.name))
        {
            itemId = gameObject.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
