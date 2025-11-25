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

    // Expose item ids so inspector can change which ids represent HP/Stamina items
    [Header("Quick item ids for debug display")] 
    [SerializeField] private string hpItemId = "holy_water";
    [SerializeField] private string staminaItemId = "elixir";

    // Specific collectible to show collected status (e.g. ManhKim)
    [Header("Single collectible status")]
    [SerializeField] private string statusCollectibleId = "ManhKim";

    void Start()
    {
        float w = 500f;
        float h = 200f;
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

        _lastPickedAmount = 1;

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

        // Instruction line
        GUILayout.Space(4);
        GUILayout.Label("You need to destroy all enemies to find power pieces.");

        // Show HP / Stamina item counts using configured ids
        int hpCount = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(hpItemId) : 0;
        int stCount = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(staminaItemId) : 0;
        GUILayout.Label($"HP ({hpItemId}): x{hpCount}");
        GUILayout.Label($"Stamina ({staminaItemId}): x{stCount}");

        // Show single collectible status
        if (!string.IsNullOrEmpty(statusCollectibleId))
        {
            bool has = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(statusCollectibleId) > 0 : false;
            GUILayout.Label($"{statusCollectibleId}: {(has ? "Have had" : "Not yet")}");
        }

        //if (GameSaveController.I != null) { 
        //    var sb = new StringBuilder(); 
        //    sb.AppendLine($"Collected: {GameSaveController.I.CollectedIds.Count}"); 
        //    foreach (var id in GameSaveController.I.CollectedIds) sb.AppendLine($" - {id}"); 
        //    GUILayout.Label(sb.ToString()); 
        //} 
        GUILayout.Label("F9 = Wipe Save");
        GUILayout.EndArea();
    }
}
