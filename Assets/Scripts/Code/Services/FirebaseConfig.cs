using UnityEngine;

[System.Serializable]
public class FirebaseConfig
{
    public string apiKey;
    public string appId;
    public string projectId;
    public string storageBucket;
    public string messagingSenderId;

    public static FirebaseConfig LoadFromStreamingAssets()
    {
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "firebase_appconfig.json");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("[FirebaseConfig] Missing firebase_appconfig.json in StreamingAssets!");
            return null;
        }
        string json = System.IO.File.ReadAllText(path);
        return JsonUtility.FromJson<FirebaseConfig>(json);
    }
}
