using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ChapterTransitionTrigger : MonoBehaviour
{
    public static ChapterTransitionTrigger Instance { get; private set; }

    [Header("Transition Settings")]
    [Tooltip("Time needed to stand in the area to pass the map (seconds)")]
    public float requiredTime = 3f;

    [Tooltip("If true, players can only start transition when there are no enemies left (GameObjects tagged 'Enemy').")]
    public bool requireNoEnemies = true;

    [Tooltip("Optional: skill id to unlock when passing this transition (calls PlayerSkillManager.UnlockSkillById)")]
    public string unlockSkillId;

    [Tooltip("If true, this trigger will persist across scenes and be reused. Scenes may provide a GameObject named 'TransitionAnchor' to reposition it.")]
    public bool persistAcrossScenes = true;

    private float timer = 0f;
    private bool isPlayerInZone = false;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Đảm bảo collider là trigger
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (!persistAcrossScenes) return;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // Bản duplicate do scene spawn → xoá
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
        if (!persistAcrossScenes) return;
        if (Instance != this) return;

        // Xoá các ChapterTransitionTrigger khác (nếu có) bằng API mới
        var others = FindObjectsByType<ChapterTransitionTrigger>(FindObjectsSortMode.None);
        foreach (var t in others)
        {
            if (t == this) continue;
            Destroy(t.gameObject);
        }

        // Reposition theo anchor của scene mới (nếu có)
        var anchor = GameObject.Find("TransitionAnchor");
        if (anchor != null)
        {
            transform.SetParent(anchor.transform.parent, false);
            transform.position = anchor.transform.position;
            transform.rotation = anchor.transform.rotation;
        }

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Reset state
        isPlayerInZone = false;
        isTransitioning = false;
        timer = 0f;
    }

    private void Update()
    {
        if (!isPlayerInZone || isTransitioning) return;

        if (requireNoEnemies)
        {
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemies != null && enemies.Length > 0)
            {
                timer = 0f;
                return;
            }
        }

        timer += Time.deltaTime;

        if (timer >= requiredTime)
        {
            isTransitioning = true;
            StartCoroutine(TransitionToNextMap());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

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
        if (!other.CompareTag("Player")) return;

        isPlayerInZone = false;
        timer = 0f;
        Debug.Log("[Transition] Player left zone.");
    }

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

        // Unlock skill nếu có
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

        ChapterManager.Instance.AdvanceMap();
        string nextScene = ChapterManager.Instance.GetNextMapName();

        yield return new WaitForSeconds(1f);

        Debug.Log($"[Transition] Loading next map: {nextScene}");
        SceneManager.LoadScene(nextScene);
    }
}
