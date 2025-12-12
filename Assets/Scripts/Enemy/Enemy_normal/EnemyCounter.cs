using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    [Header("Reference")]
    public PlayerSkillManager skillManager; // kept for backward compatibility but not used for unlocking here
    public GameObject chapterTransitionTrigger;

    [Header("Setting")]
    public float checkInterval = 1.5f;

    private float timer = 0f;
    private bool eventTriggered = false;

    void Start()
    {
        if (chapterTransitionTrigger != null)
        {
            chapterTransitionTrigger.SetActive(false);
        }

        // If there's a persistent ChapterTransitionTrigger already in scene, ensure it's initially disabled
        if (ChapterTransitionTrigger.Instance != null)
        {
            ChapterTransitionTrigger.Instance.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (eventTriggered) return;

        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;

            // Prefer counting by tag, fallback to EnemyStats type if tags are not set
            int remaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
            if (remaining == 0)
            {
                var byType = FindObjectsOfType<EnemyStats>();
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

            //Debug.Log($"🧮 Enemy còn lại: {remaining}");

            if (remaining == 0)
            {

                if (ChapterTransitionTrigger.Instance != null)
                {
                    ChapterTransitionTrigger.Instance.gameObject.SetActive(true);
                    Debug.Log("🌀 Vùng chuyển cảnh (persistent) đã xuất hiện!");
                }
                else if (chapterTransitionTrigger != null)
                {
                    chapterTransitionTrigger.SetActive(true);
                    Debug.Log("🌀 Vùng chuyển cảnh đã xuất hiện!");
                }
                else
                {
                    Debug.LogWarning("[EnemyCounter] Không có chapterTransitionTrigger để bật.");
                }

                // Optional feedback to player
                UI_Toasts.Show("Gate unlocked! Stand in the portal to proceed.");

                eventTriggered = true;
            }
        }
    }
}
