using UnityEngine;
using System;

[DisallowMultipleComponent]
public class CollectibleId : MonoBehaviour
{
    [SerializeField] private string uniqueId;
    public string Id => uniqueId;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString("N");
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
