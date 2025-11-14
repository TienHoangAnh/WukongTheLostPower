using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugHUD : MonoBehaviour
{
    private Rect _rect = new Rect(100, 400, 500, 220);

    // lưu item vừa nhặt
    private string _lastPickedName;
    private float _lastPickedTime;
    [SerializeField] private float pickedMsgDuration = 4f; // 4s rồi mờ đi

    void OnEnable()
    {
        CollectiblePickup.OnPicked += HandlePicked;
    }

    void OnDisable()
    {
        CollectiblePickup.OnPicked -= HandlePicked;
    }

    private void HandlePicked(string displayName, string id)
    {
        _lastPickedName = displayName;
        _lastPickedTime = Time.time;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (Input.GetKeyDown(KeyCode.F9))
            GameSaveController.I?.WipeAndReload();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(_rect, GUI.skin.box);
        GUILayout.Label("<b>DEBUG HUD</b>");
        GUILayout.Label($"persistentDataPath:\n{Application.persistentDataPath}");

        // Hiện dòng "Bạn nhặt được" trong một khoảng thời gian
        if (!string.IsNullOrEmpty(_lastPickedName))
        {
            var elapsed = Time.time - _lastPickedTime;
            if (elapsed <= pickedMsgDuration)
            {
                GUILayout.Label($"Bạn nhặt được: <b>{_lastPickedName}</b>");
            }
        }

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
