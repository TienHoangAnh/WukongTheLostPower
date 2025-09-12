using UnityEngine;

public class BossBehaviorSelector : MonoBehaviour
{
    public GameObject meleeBossPrefab;
    public GameObject rangedBossPrefab;
    public GameObject hybridBossPrefab; // Thêm prefab Hybrid
    public Transform spawnPoint;

    void Start()
    {
        string style = FindFirstObjectByType<PlayerBehaviorTracker>().GetPlaystyle();
        Debug.Log("🔍 Phong cách chơi người chơi: " + style);

        GameObject boss = null;

        if (style == "Melee")
        {
            boss = Instantiate(rangedBossPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("🧠 Sinh Boss tầm xa (vì người chơi hay đánh cận chiến)");
        }
        else if (style == "Ranged")
        {
            boss = Instantiate(meleeBossPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("🧠 Sinh Boss cận chiến (vì người chơi hay đánh xa)");
        }
        else
        {
            boss = Instantiate(hybridBossPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("🧠 Sinh Boss hỗn hợp (người chơi cân bằng giữa đánh xa & gần)");
        }

        if (boss == null)
        {
            Debug.LogError("❌ Không tạo được boss! Prefab bị null?");
        }
        else
        {
            boss.SetActive(true); // đảm bảo boss không bị tắt
            Debug.Log("✅ Đã tạo boss: " + boss.name);
        }

    }

}