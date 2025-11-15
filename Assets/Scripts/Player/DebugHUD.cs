using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugHUD : MonoBehaviour
{
    private Rect _rect;

    private string _lastPickedName;
    private string _lastPickedId;
    private int _lastPickedAmount;
    private int _lastPickedTotal;
    private float _lastPickedTime;


    [SerializeField] private float pickedMsgDuration = 5f;

    void Start()
    {
        float w = 250f;
        float h = 250f;
        _rect = new Rect(Screen.width - w - 10, 50, w, h);
    }

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
        _lastPickedId = id;
        _lastPickedTime = Time.time;

        // hiện tại CollectiblePickup luôn nhặt 1
        _lastPickedAmount = 1;

        // tổng số hiện có trong save data
        _lastPickedTotal = (GameSaveController.I != null)
            ? GameSaveController.I.GetCollectedCount(id)
            : 0;
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
        GUILayout.Label("<b>Main Task</b>");

        if (!string.IsNullOrEmpty(_lastPickedName))
        {
            var elapsed = Time.time - _lastPickedTime;
            if (elapsed <= pickedMsgDuration)
            {
                GUILayout.Label(
                    $"You picked up: <b>{_lastPickedName}</b> " +
                    $"(+{_lastPickedAmount}, total: {_lastPickedTotal})"
                );
            }
        }
        if (GameSaveController.I != null) { 
            var sb = new StringBuilder(); 
            sb.AppendLine($"Collected: {GameSaveController.I.CollectedIds.Count}"); 
            foreach (var id in GameSaveController.I.CollectedIds) sb.AppendLine($" - {id}"); 
            GUILayout.Label(sb.ToString()); 
        } 
        GUILayout.Label("F9 = Wipe Save");
        GUILayout.EndArea();
    }
}
