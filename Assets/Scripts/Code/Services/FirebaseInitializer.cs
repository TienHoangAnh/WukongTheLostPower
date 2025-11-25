using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

/// <summary>
/// Simple MonoBehaviour-based Firebase bootstrapper that initialises FirebaseApp and
/// anonymous authentication on Awake, and persists itself across scenes.
/// </summary>
public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseApp App;
    public static FirebaseAuth Auth;
    public static FirebaseUser User;

    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        await Init();
    }

    /// <summary>
    /// Ensures Firebase dependencies are available, initialises default app and auth,
    /// and signs in anonymously if no user is currently authenticated.
    /// </summary>
    public static async Task Init()
    {
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep == DependencyStatus.Available)
        {
            App = FirebaseApp.DefaultInstance;
            Auth = FirebaseAuth.DefaultInstance;

            if (Auth.CurrentUser == null)
            {
                await Auth.SignInAnonymouslyAsync();
            }

            User = Auth.CurrentUser;
            Debug.Log($"[FirebaseInitializer] Firebase ready. uid={User?.UserId}");
        }
        else
        {
            Debug.LogError($"[FirebaseInitializer] Firebase dependencies not available: {dep}");
        }
    }
}
