using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    [Header("Tham chiếu")]
    public PlayerSkillManager skillManager;
    public GameObject chapterTransitionTrigger; // Kéo vùng trigger vào đây

    [Header("Cài đặt")]
    public float checkInterval = 1.5f;

    private float timer = 0f;
    private bool eventTriggered = false;

    void Start()
    {
        if (chapterTransitionTrigger != null)
        {
            chapterTransitionTrigger.SetActive(false);
        }
    }


    void Update()
    {
        if (eventTriggered) return;

        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;

            int remaining = GameObject.FindGameObjectsWithTag("Enemy").Length;
            Debug.Log($"🧮 Enemy còn lại: {remaining}");

            if (remaining == 0)
            {
                // Mở khóa kỹ năng Heal
                skillManager?.UnlockSkill(1, new HealSkill());
                Debug.Log("🔓 Kỹ năng Heal đã được mở khóa!");

                // Kích hoạt vùng chuyển cảnh
                if (chapterTransitionTrigger != null)
                {
                    chapterTransitionTrigger.SetActive(true);
                    Debug.Log("🌀 Vùng chuyển cảnh đã xuất hiện!");
                }

                eventTriggered = true;
            }
        }
    }
}
