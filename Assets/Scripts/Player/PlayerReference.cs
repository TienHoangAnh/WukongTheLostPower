using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    public static Transform Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = transform;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
