using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseApp App;
    public static FirebaseAuth Auth;
    public static FirebaseUser User;

    async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        await Init();
    }

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
            Debug.Log($"Firebase ready. uid={User?.UserId}");
        }
        else Debug.LogError($"Firebase deps: {dep}");
    }
}
