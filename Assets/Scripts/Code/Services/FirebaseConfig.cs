using System.IO;
using UnityEngine;

/// <summary>
/// Serializable representation of firebase_appconfig.json stored in StreamingAssets.
/// Contains the basic fields required to create a Firebase AppOptions instance.
/// </summary>
[System.Serializable]
public class FirebaseConfig
{
    public string apiKey;
    public string appId;
    public string projectId;
    public string storageBucket;
    public string messagingSenderId;

    /// <summary>
    /// Loads FirebaseConfig from StreamingAssets/firebase_appconfig.json.
    /// Logs an error and returns null if the file is missing or cannot be parsed.
    /// </summary>
    public static FirebaseConfig LoadFromStreamingAssets()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "firebase_appconfig.json");
        if (!File.Exists(path))
        {
            Debug.LogError("[FirebaseConfig] Missing firebase_appconfig.json in StreamingAssets.");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<FirebaseConfig>(json);
    }
}
