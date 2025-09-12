#if FIREBASE_ENABLED
using System;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public static class FirebaseRuntime
{
    public static FirebaseApp App { get; private set; }
    public static FirebaseAuth Auth { get; private set; }
    public static FirebaseFirestore Db { get; private set; }
    private static bool _ready;

    public static async Task EnsureInitializedAsync()
    {
        if (_ready) return;

        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep != DependencyStatus.Available)
            throw new Exception($"Firebase deps: {dep}");

        // 1) Create app from StreamingAssets config (không đụng DefaultInstance)
        if (App == null)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "firebase_appconfig.json");
            if (!File.Exists(path))
                throw new Exception($"Missing config at {path}");

            var json = File.ReadAllText(path);
            var cfg = JsonUtility.FromJson<FirebaseConfig>(json);

            var opts = new AppOptions
            {
                ApiKey = cfg.apiKey,
                AppId = cfg.appId,
                ProjectId = cfg.projectId,
                StorageBucket = cfg.storageBucket
            };

            // Nếu app tên mặc định đã tồn tại do lần chạy trước → dùng tên khác
            try { App = FirebaseApp.Create(opts); }
            catch
            {
                // Fallback: thử lấy app mặc định nếu đã có (trường hợp editor domain reload)
                try { App = FirebaseApp.GetInstance("[DEFAULT]"); }
                catch (Exception e) { throw new Exception("Cannot create or get FirebaseApp", e); }
            }
        }

        // 2) Auth & Firestore gắn với app vừa tạo (không dùng DefaultInstance)
        Auth = FirebaseAuth.GetAuth(App);
        Db = FirebaseFirestore.GetInstance(App);

        if (Auth.CurrentUser == null)
            await Auth.SignInAnonymouslyAsync();

        _ready = true;
        Debug.Log($"[FirebaseRuntime] Ready. uid={Auth.CurrentUser?.UserId}");
    }
}

#endif
