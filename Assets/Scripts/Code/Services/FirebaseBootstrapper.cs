#if FIREBASE_ENABLED
using UnityEngine;

public class FirebaseBootstrapper : MonoBehaviour
{
    private async void Awake()
    {
        DontDestroyOnLoad(this);

        // Giảm ồn log từ Firebase (ẩn Info/Debug)
        Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Warning;

        try
        {
            await FirebaseRuntime.EnsureInitializedAsync();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FirebaseBootstrapper] Init failed: {e}");
        }
    }
}
#endif
