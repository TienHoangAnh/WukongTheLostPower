using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAnchor : MonoBehaviour
{
    public static Transform Current;

    void OnEnable()
    {
        Current = transform;
    }

    void OnDisable()
    {
        if (Current == transform)
            Current = null;
    }
}
