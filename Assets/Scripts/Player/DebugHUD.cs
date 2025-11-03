using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugHUD : MonoBehaviour
{
    private Rect _rect = new Rect(20, 20, 460, 260);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.F9))
            GameSaveController.I?.WipeAndReload();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(_rect, GUI.skin.box);
        GUILayout.Label("<b>DEBUG HUD</b>");
        GUILayout.Label($"persistentDataPath:\n{Application.persistentDataPath}");
        if (GameSaveController.I != null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Collected: {GameSaveController.I.CollectedIds.Count}");
            foreach (var id in GameSaveController.I.CollectedIds)
                sb.AppendLine($" - {id}");
            GUILayout.Label(sb.ToString());
        }
        GUILayout.Label("Keys: R = Reload, F9 = Wipe Save");
        GUILayout.EndArea();
    }
}
