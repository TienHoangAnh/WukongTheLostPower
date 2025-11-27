using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ChapterTransitionTrigger : MonoBehaviour
{
    public static ChapterTransitionTrigger Instance { get; private set; }

    [Header("Transition Settings")]
    [Tooltip("Time the player needs to stand in this area before transitioning (in seconds).")]
    public float requiredTime = 3f;

    [Tooltip("If true, transition can only start when there are no GameObjects tagged 'Enemy'.")]
    public bool requireNoEnemies = true;

    [Tooltip("Optional skill id to unlock when passing this transition (calls PlayerSkillManager.UnlockSkillById).")]
    public string unlockSkillId;

    [Tooltip("If true, this trigger will persist across scenes. Scenes may provide a 'TransitionAnchor' to reposition it.")]
    public bool persistAcrossScenes = true;

    [Header("Per-map required item (optional)")]
    [Tooltip("Optional mapping: map index -> required collectible id. Index is the map number (1-based). Leave blank to not require an item.")]
    public string[] requiredItemForMaps = new string[0];

    private float timer = 0f;
    private bool isPlayerInZone = false;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Ensure collider is configured as trigger
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        if (!persistAcrossScenes)
            return;

        // Simple singleton pattern for a persistent transition trigger
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!persistAcrossScenes)
            return;
        if (Instance != this)
            return;

        // Remove any duplicate ChapterTransitionTrigger objects if they were created by the new scene
        var others = FindObjectsByType<ChapterTransitionTrigger>(FindObjectsSortMode.None);
        foreach (var t in others)
        {
            if (t == this)
                continue;

            // Copy per-scene configuration from the scene's trigger into the persistent instance
            // so the persistent object reflects the new scene's required items and settings.
            try
            {
                this.requiredTime = t.requiredTime;
                this.requireNoEnemies = t.requireNoEnemies;
                this.unlockSkillId = t.unlockSkillId;
                this.persistAcrossScenes = t.persistAcrossScenes;

                if (t.requiredItemForMaps != null && t.requiredItemForMaps.Length > 0)
                    this.requiredItemForMaps = (string[])t.requiredItemForMaps.Clone();
            }
            catch
            {
                // Ignore any errors during copy; continue to destroy duplicate.
            }

            Destroy(t.gameObject);
        }

        // Reposition to optional scene anchor
        var anchor = GameObject.Find("TransitionAnchor");
        if (anchor != null)
        {
            transform.SetParent(anchor.transform.parent, false);
            transform.position = anchor.transform.position;
            transform.rotation = anchor.transform.rotation;
        }

        var col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;

        // Reset state when a new scene is loaded
        isPlayerInZone = false;
        isTransitioning = false;
        timer = 0f;
    }

    private void Update()
    {
        if (!isPlayerInZone || isTransitioning)
            return;

        // Do not progress if required item is missing
        if (!HasRequiredItem())
            return;

        // Optionally require all enemies to be cleared before transitioning
        if (requireNoEnemies)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies != null && enemies.Length > 0)
            {
                timer = 0f;
                return;
            }
        }

        // Accumulate time inside the zone
        timer += Time.deltaTime;

        if (timer >= requiredTime)
        {
            isTransitioning = true;
            StartCoroutine(TransitionToNextMap());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Immediate check for required item and enemies before starting the timer
        if (!HasRequiredItem())
        {
            var reqId = GetRequiredItemForCurrentMap();
            string display = reqId;

            if (!string.IsNullOrEmpty(reqId) && InventoryDatabase.I != null)
            {
                var asset = InventoryDatabase.I.GetById(reqId);
                if (asset != null)
                    display = string.IsNullOrEmpty(asset.displayName) ? reqId : asset.displayName;
            }

            Debug.Log($"[Transition] Cannot start transition: required item not collected: {reqId}");
            UI_Toasts.Show($"You need to find {display} to unlock the passage");
            return;
        }

        if (requireNoEnemies)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies != null && enemies.Length > 0)
            {
                Debug.Log("[Transition] Cannot start transition: enemies remain.");
                UI_Toasts.Show("Clear all enemies to unlock the passage");
                return;
            }
        }

        isPlayerInZone = true;
        timer = 0f;
        Debug.Log("[Transition] Player entered zone.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInZone = false;
        timer = 0f;
        Debug.Log("[Transition] Player left zone.");
    }

    /// <summary>
    /// Handles transitioning to the next map, optionally unlocking a skill before loading the next scene.
    /// </summary>
    private System.Collections.IEnumerator TransitionToNextMap()
    {
        Debug.Log("[Transition] Changing scene...");

        if (ChapterManager.Instance == null)
        {
            Debug.LogError("[Transition] ChapterManager missing!");
            yield break;
        }

        if (!ChapterManager.Instance.HasNextMap())
        {
            Debug.Log("[Transition] Reached final map — show ending or credits.");
            yield break;
        }

        // Unlock skill associated with this transition, if any
        if (!string.IsNullOrWhiteSpace(unlockSkillId))
        {
            var skillMgr = FindFirstObjectByType<PlayerSkillManager>();
            if (skillMgr != null)
            {
                skillMgr.UnlockSkillById(unlockSkillId);
            }
            else
            {
                Debug.LogWarning("[Transition] PlayerSkillManager not found to unlock skill.");
            }
        }

        // Advance chapter state and load next scene
        ChapterManager.Instance.AdvanceMap();
        string nextScene = ChapterManager.Instance.GetNextMapName();

        yield return new WaitForSeconds(1f);

        Debug.Log($"[Transition] Loading next map: {nextScene}");
        if (LoadingScreen.I != null)
            LoadingScreen.LoadScene(nextScene);
        else
            SceneManager.LoadScene(nextScene);
    }

    /// <summary>
    /// Returns the required collectible id for the current map, or null if none is defined.
    /// </summary>
    public string GetRequiredItemForCurrentMap()
    {
        if (ChapterManager.Instance == null)
            return null;

        int mapIndex = ChapterManager.Instance.currentMap;
        // Map index is 1-based. requiredItemForMaps is 0-based where element 0 == map 1.
        int arrIndex = mapIndex - 1;

        if (requiredItemForMaps == null || requiredItemForMaps.Length < 1)
            return null;
        if (arrIndex < 0 || arrIndex >= requiredItemForMaps.Length)
            return null;

        return requiredItemForMaps[arrIndex];
    }

    /// <summary>
    /// Returns true if no required item is configured or the player has already collected it.
    /// </summary>
    private bool HasRequiredItem()
    {
        var req = GetRequiredItemForCurrentMap();
        if (string.IsNullOrEmpty(req))
            return true; // no requirement configured

        if (GameSaveController.I == null)
            return false;

        return GameSaveController.I.GetCollectedCount(req) > 0;
    }
}
