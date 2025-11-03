using UnityEngine;

public class EnemyId : MonoBehaviour
{
    public string id;

#if UNITY_EDITOR
    [ContextMenu("Ensure Id")]
    private void EnsureId()
    {
        if (string.IsNullOrEmpty(id)) id = System.Guid.NewGuid().ToString();
    }
#endif
}
