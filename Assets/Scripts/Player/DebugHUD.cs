using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class DebugHUD : MonoBehaviour
{
    public static DebugHUD Instance { get; private set; }

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
    // statusCollectibleId is optional; per-map required item will be used if available
    [SerializeField] private string statusCollectibleId = "";

    // runtime copy of the collectible id to show (updated on scene load)
    private string _runtimeStatusCollectibleId;

    // Message log (timed messages)
    private struct Msg
    {
        public string text;
        public float expiry;

        public Msg(string t, float e)
        {
            text = t;
            expiry = e;
        }
    }

    private readonly List<Msg> _messages = new List<Msg>();

    // Tracking for state changes
    private int _prevEnemyCount = -1;
    private int _prevCollectedHp = -1;
    private int _prevCollectedSt = -1;
    private int _prevCollectedRequired = -1;
    private int _prevSkillsCount = -1;
    private float _prevPlayerHp = -1f;
    private int _prevDeathCount = -1;

    // Track current map index to detect changes
    private int _prevMapIndex = -1;

    [Header("HUD Settings")]
    [Tooltip("How long general messages stay in the debug box (seconds)")]
    [SerializeField] private float generalMsgDuration = 5f;

    [Header("Dev Hotkeys")]
    [Tooltip("Enable L to reload current scene & reset collected items in-memory (dev only).")]
    [SerializeField] private bool enableDebugHotkeys = true;

    // Playtime tracking
    // mirrors SaveRuntime.Current.playTimeSeconds
    private float _playTimeSeconds = 0f;
    private bool _isTiming = false;

    [Tooltip("How often (seconds) to persist playtime to save/cloud")]
    [SerializeField] private float playtimeSaveInterval = 5f;

    private float _playtimeSaveTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        float w = 500f;
        float h = 200f;
        _rect = new Rect(Screen.width - w - 10, 50, w, h);

        // initialize runtime id from current scene's trigger if present
        RefreshStatusCollectibleForCurrentMap();

        // initialize previous tracking values
        _prevEnemyCount = CountEnemies();
        _prevCollectedHp = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(hpItemId) : 0;
        _prevCollectedSt = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(staminaItemId) : 0;
        _prevSkillsCount = SaveRuntime.Current != null && SaveRuntime.Current.skillsUnlocked != null
            ? SaveRuntime.Current.skillsUnlocked.Count
            : 0;
        _prevPlayerHp = PlayerStats.Instance != null ? PlayerStats.Instance.currentHealth : -1f;

        // death count
        _prevDeathCount = SaveRuntime.Current != null ? SaveRuntime.Current.deathCount : 0;

        // playtime init from save runtime if present
        _playTimeSeconds = SaveRuntime.Current != null ? SaveRuntime.Current.playTimeSeconds : 0f;

        // initialize map tracking
        _prevMapIndex = ChapterManager.Instance != null ? ChapterManager.Instance.currentMap : -1;

        // start timing automatically if current scene is a map (not menu)
        var active = SceneManager.GetActiveScene();
        if (IsMapScene(active))
        {
            StartPlaytime();
            AddMessage($"Playtime tracking started. Current: {FormatTime(_playTimeSeconds)}", 3f);
        }

        // also seed a status message
        AddMessage($"Debug HUD initialized. Enemies: {_prevEnemyCount}", 3f);
    }

    private void OnEnable()
    {
        CollectiblePickup.OnPicked += HandlePicked;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        CollectiblePickup.OnPicked -= HandlePicked;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Persist playtime when the HUD is disabled (e.g. on quit)
        PersistPlaytime();
    }

    private void OnApplicationQuit()
    {
        PersistPlaytime();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            PersistPlaytime();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // update runtime status collectible when a new scene loads so persistent System objects
        // reflect the new map's required item
        // Delay one frame to allow persistent ChapterTransitionTrigger to copy per-scene settings first
        StartCoroutine(RefreshStatusCollectibleNextFrame());

        // refresh tracked state after scene change
        int ec = CountEnemies();
        if (ec != _prevEnemyCount)
        {
            _prevEnemyCount = ec;
            AddMessage($"Scene loaded. Enemy count: {ec}", 3f);
        }

        _prevCollectedHp = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(hpItemId) : 0;
        _prevCollectedSt = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(staminaItemId) : 0;
        _prevPlayerHp = PlayerStats.Instance != null ? PlayerStats.Instance.currentHealth : -1f;

        // Start playtime when entering any playable map scene. Do not stop on leaving — keep accumulating until quit.
        if (IsMapScene(scene))
        {
            StartPlaytime();
            AddMessage($"Entered {scene.name}. Playtime tracking active.", 3f);
        }
    }

    private IEnumerator RefreshStatusCollectibleNextFrame()
    {
        // wait one frame so other sceneLoaded handlers (e.g. ChapterTransitionTrigger) can run
        yield return null;
        RefreshStatusCollectibleForCurrentMap();
    }

    // Consider any scene whose name starts with "Map" as a playable map scene. Adjust if needed.
    private bool IsMapScene(Scene s)
    {
        if (!s.IsValid()) return false;
        return s.name.StartsWith("Map", System.StringComparison.OrdinalIgnoreCase);
    }

    private bool IsMap1Scene(Scene s)
    {
        if (!s.IsValid()) return false;
        // left for compatibility with older checks
        return string.Equals(s.name, "Map1", System.StringComparison.OrdinalIgnoreCase);
    }

    private void StartPlaytime()
    {
        if (_isTiming) return; // already running

        _isTiming = true;
        _playtimeSaveTimer = 0f;

        // ensure SaveRuntime has initialised structure
        SaveRuntime.EnsureInitialized();
        if (SaveRuntime.Current.playTimeSeconds <= 0f)
            SaveRuntime.Current.playTimeSeconds = _playTimeSeconds;
    }

    private void PersistPlaytime()
    {
        SaveRuntime.EnsureInitialized();
        SaveRuntime.Current.playTimeSeconds = _playTimeSeconds;
        _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
        Debug.Log($"[DebugHUD] Persisted playtime: {FormatTime(_playTimeSeconds)}");
    }

    private void StopAndPersistPlaytime()
    {
        if (!_isTiming) return;
        _isTiming = false;
        PersistPlaytime();
    }

    private void RefreshStatusCollectibleForCurrentMap()
    {
        var trigger = ChapterTransitionTrigger.Instance;
        if (trigger != null)
        {
            var req = trigger.GetRequiredItemForCurrentMap();
            if (!string.IsNullOrEmpty(req))
            {
                _runtimeStatusCollectibleId = req;
                return;
            }
        }

        // fallback to inspector value
        _runtimeStatusCollectibleId = statusCollectibleId;
    }

    private void HandlePicked(string displayName, string id)
    {
        _lastPickedName = displayName;
        _lastPickedId = id;
        _lastPickedTime = Time.time;

        _lastPickedAmount = 1;

        _lastPickedTotal = GameSaveController.I != null
            ? GameSaveController.I.GetCollectedCount(id)
            : 0;

        AddMessage($"Picked up: {displayName} (+{_lastPickedAmount}, total {_lastPickedTotal})", pickedMsgDuration);

        // If this is a required item for the current map, highlight it
        if (!string.IsNullOrEmpty(_runtimeStatusCollectibleId) && id == _runtimeStatusCollectibleId)
        {
            AddMessage($"You found the required item for this map: {displayName}", generalMsgDuration + 2f);
        }

        // Update tracked counts for HP/stamina quick-items
        if (id == hpItemId)
        {
            int now = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(hpItemId) : 0;
            if (now != _prevCollectedHp)
            {
                _prevCollectedHp = now;
                AddMessage($"HP item ({hpItemId}) count: x{now}", generalMsgDuration);
            }
        }

        if (id == staminaItemId)
        {
            int now = GameSaveController.I != null ? GameSaveController.I.GetCollectedCount(staminaItemId) : 0;
            if (now != _prevCollectedSt)
            {
                _prevCollectedSt = now;
                AddMessage($"Stamina item ({staminaItemId}) count: x{now}", generalMsgDuration);
            }
        }
    }

    private void Update()
    {
        // Detect map index change and refresh runtime required item immediately
        if (ChapterManager.Instance != null)
        {
            int current = ChapterManager.Instance.currentMap;
            if (current != _prevMapIndex)
            {
                _prevMapIndex = current;
                RefreshStatusCollectibleForCurrentMap();
                AddMessage($"Map changed: {current}", 2f);
            }
        }

        // Playtime ticking
        if (_isTiming)
        {
            _playTimeSeconds += Time.deltaTime;
            _playtimeSaveTimer += Time.deltaTime;

            // persist periodically
            if (_playtimeSaveTimer >= playtimeSaveInterval)
            {
                _playtimeSaveTimer = 0f;
                SaveRuntime.EnsureInitialized();
                SaveRuntime.Current.playTimeSeconds = _playTimeSeconds;
                _ = CloudSaveManager.SaveNow(SaveRuntime.Current);
            }
        }

        // Poll enemy count changes
        int enemies = CountEnemies();
        if (_prevEnemyCount >= 0 && enemies == 0 && _prevEnemyCount > 0)
        {
            AddMessage("All enemies defeated! Gate unlocked.", generalMsgDuration + 2f);
        }
        _prevEnemyCount = enemies;

        // Poll skill unlocks via SaveRuntime changes
        int skillsNow = SaveRuntime.Current != null && SaveRuntime.Current.skillsUnlocked != null
            ? SaveRuntime.Current.skillsUnlocked.Count
            : 0;

        if (skillsNow > _prevSkillsCount)
        {
            // find newly unlocked ids
            if (_prevSkillsCount >= 0 && SaveRuntime.Current != null && SaveRuntime.Current.skillsUnlocked != null)
            {
                for (int i = 0; i < SaveRuntime.Current.skillsUnlocked.Count; i++)
                {
                    // If index beyond previous count, treat as new
                    if (i >= _prevSkillsCount)
                    {
                        string sid = SaveRuntime.Current.skillsUnlocked[i];
                        AddMessage($"Skill unlocked: {sid}", generalMsgDuration + 2f);
                    }
                }
            }
        }
        _prevSkillsCount = skillsNow;

        // Low HP warning
        var ps = PlayerStats.Instance;
        if (ps != null)
        {
            float hp = ps.currentHealth;
            if (_prevPlayerHp >= 0 && hp < 20f && _prevPlayerHp >= 20f)
            {
                AddMessage("Warning: HP below 20%! Use an HP item (quick-slot) to recover.", generalMsgDuration + 3f);
            }
            _prevPlayerHp = hp;
        }

        // Detect death count changes (PlayerStats.Die updates SaveRuntime.Current.deathCount)
        int nowDeaths = SaveRuntime.Current != null ? SaveRuntime.Current.deathCount : 0;
        if (_prevDeathCount >= 0 && nowDeaths > _prevDeathCount)
        {
            AddMessage($"You died ({nowDeaths} total).", generalMsgDuration + 2f);
        }
        _prevDeathCount = nowDeaths;

        // Transition readiness: show small status message when conditions met/not met
        var trigger = ChapterTransitionTrigger.Instance;
        if (trigger != null)
        {
            bool hasRequired = false;
            var req = trigger.GetRequiredItemForCurrentMap();
            if (string.IsNullOrEmpty(req)) hasRequired = true;
            else hasRequired = (GameSaveController.I != null && GameSaveController.I.GetCollectedCount(req) > 0);

            bool enemiesCleared = CountEnemies() == 0;

            if (hasRequired && enemiesCleared)
            {
                // ready
                int nowReady = 1;
                if (_prevCollectedRequired != nowReady)
                {
                    AddMessage("Passage ready: You can enter the portal to proceed.", generalMsgDuration + 2f);
                }
                _prevCollectedRequired = nowReady;
            }
            else
            {
                int nowReady = 0;
                if (_prevCollectedRequired != nowReady)
                {
                    // describe why locked
                    if (!hasRequired)
                    {
                        string display = req ?? "required item";
                        if (InventoryDatabase.I != null && !string.IsNullOrEmpty(req))
                        {
                            var a = InventoryDatabase.I.GetById(req);
                            if (a != null && !string.IsNullOrEmpty(a.displayName)) display = a.displayName;
                        }
                        AddMessage($"Passage locked: find {display}.", generalMsgDuration + 2f);
                    }
                    else if (!enemiesCleared)
                    {
                        AddMessage("Passage locked: clear all enemies first.", generalMsgDuration + 2f);
                    }
                }
                _prevCollectedRequired = nowReady;
            }
        }

        // Cleanup expired messages
        float nowt = Time.time;
        for (int i = _messages.Count - 1; i >= 0; i--)
        {
            if (_messages[i].expiry > 0f && _messages[i].expiry < nowt)
                _messages.RemoveAt(i);
        }
    }

    private int CountEnemies()
    {
        int remaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (remaining == 0)
        {
            var byType = FindObjectsByType<EnemyStats>(FindObjectsSortMode.None);
            if (byType != null)
            {
                int alive = 0;
                foreach (var e in byType)
                {
                    if (e != null && e.gameObject.activeInHierarchy) alive++;
                }
                remaining = alive;
            }
        }
        return remaining;
    }

    private void AddMessage(string text, float duration)
    {
        float expiry = duration > 0 ? Time.time + duration : 0f;
        _messages.Add(new Msg(text, expiry));
        Debug.Log("[DebugHUD] " + text);
    }

    // Public helper so other systems (UI_Toasts) can forward messages into the debug HUD
    public static void ShowDebugMessage(string text, float duration)
    {
        if (Instance != null)
            Instance.AddMessage(text, duration);
        else
            Debug.Log("[DebugHUD] " + text);
    }

    private static string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        int mins = s / 60;
        int secs = s % 60;
        return string.Format("{0:00}:{1:00}", mins, secs);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(_rect, GUI.skin.box);

        int mapIndex = ChapterManager.Instance != null ? ChapterManager.Instance.currentMap : -1;
        GUILayout.Label($"<b>Playtime: {FormatTime(_playTimeSeconds)}</b> <b>Map: {mapIndex}</b> <b>Main Task</b>");

        // If this is the final map, show a clear victory message and skip other debug labels
        if (ChapterManager.Instance != null && !ChapterManager.Instance.HasNextMap())
        {
            GUIStyle victoryStyle = new GUIStyle(GUI.skin.label);
            victoryStyle.fontStyle = FontStyle.Bold;
            victoryStyle.alignment = TextAnchor.MiddleCenter;
            victoryStyle.fontSize = 16;
            victoryStyle.normal.textColor = Color.yellow;
            victoryStyle.richText = false;

            GUILayout.Space(6);
            GUILayout.Label("Wukong has regained his strength. Complete the final challenge to win.!", victoryStyle);
            GUILayout.Space(6);

            GUILayout.EndArea();
            return;
        }

        foreach (var m in _messages)
        {
            GUILayout.Label(m.text);
        }

        GUILayout.Space(2);

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

        // Show playtime and death count
        int deaths = SaveRuntime.Current != null ? SaveRuntime.Current.deathCount : 0;
        GUILayout.Label($"Deaths: {deaths}");

        // Show melee/ranged attack counts and inferred playstyle
        var pb = PlayerBehaviorTracker.Instance;
        int meleeAtt = 0;
        int rangedAtt = 0;
        string playstyle = "Unknown";

        if (pb != null)
        {
            meleeAtt = pb.meleeCount;
            rangedAtt = pb.rangedCount;
            playstyle = pb.GetPlaystyle();
        }
        else
        {
            // fallback to SaveRuntime values if tracker isn't present
            if (SaveRuntime.Current != null)
            {
                meleeAtt = SaveRuntime.Current.meleeCount;
                rangedAtt = SaveRuntime.Current.rangedCount;
            }
            // fallback to PlayerPrefs playstyle if available
            if (PlayerPrefs.HasKey("Playstyle")) playstyle = PlayerPrefs.GetString("Playstyle");
        }

        GUILayout.Label($"Melee: {meleeAtt} | Ranged: {rangedAtt} (Playstyle: {playstyle})");

        // Show single collectible status: prefer per-map required item from ChapterTransitionTrigger
        var trigger = ChapterTransitionTrigger.Instance;
        string reqId = null;
        if (trigger != null)
            reqId = trigger.GetRequiredItemForCurrentMap();

        // if trigger doesn't provide a per-map id, use the runtime value which was refreshed on scene load
        if (string.IsNullOrEmpty(reqId))
            reqId = _runtimeStatusCollectibleId;

        if (!string.IsNullOrEmpty(reqId))
        {
            bool has = GameSaveController.I != null && GameSaveController.I.GetCollectedCount(reqId) > 0;
            var display = reqId;

            if (InventoryDatabase.I != null)
            {
                var asset = InventoryDatabase.I.GetById(reqId);
                if (asset != null && !string.IsNullOrEmpty(asset.displayName))
                    display = asset.displayName;
            }

            GUILayout.Label($"{display}: {(has ? "Have had" : "Not yet")} ");

            // If the required collectible for this map is not yet collected, show a clear instruction
            if (!has)
            {
                // Try to include the skill id that this transition unlocks (if any)
                string skillNote = "";
                var triggerInstance = ChapterTransitionTrigger.Instance;
                if (triggerInstance != null && !string.IsNullOrWhiteSpace(triggerInstance.unlockSkillId))
                {
                    skillNote = $" (will unlock skill: {triggerInstance.unlockSkillId})";
                }

                GUILayout.Space(4);
                GUILayout.Label($"Objective: Find the {display} to unlock the passage{skillNote}.");
                GUILayout.Label("Go to the transition area after collecting the piece to proceed.");
            }
        }

        GUILayout.EndArea();
    }
}
