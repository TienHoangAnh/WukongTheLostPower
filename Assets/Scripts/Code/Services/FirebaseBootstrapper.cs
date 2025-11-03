#if FIREBASE_ENABLED
using UnityEngine;

public class FirebaseBootstrapper : MonoBehaviour
{
    private async void Awake()
    {
        DontDestroyOnLoad(this);

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
