using UnityEngine;

public class BossBehaviorSelector : MonoBehaviour
{
    public GameObject meleeBossPrefab;
    public GameObject rangedBossPrefab;
    public GameObject hybridBossPrefab;
    public Transform spawnPoint;

    void Start()
    {
        string style = FindFirstObjectByType<PlayerBehaviorTracker>().GetPlaystyle();
        Debug.Log("🔍 Player play style: " + style);

        GameObject boss = null;

        if (style == "Melee")
        {
            boss = Instantiate(rangedBossPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("🧠 Spawn long-range Boss (because players often fight in close combat)");
        }
        else if (style == "Ranged")
        {
            boss = Instantiate(meleeBossPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("🧠 Spawn melee boss (because the player often attacks from a distance)");
        }
        else
        {
            boss = Instantiate(hybridBossPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("🧠 Mixed Boss Spawn (player balances between ranged & melee)");
        }

        if (boss == null)
        {
            Debug.LogError("❌ Unable to create boss! Prefab is null?");
        }
        else
        {
            boss.SetActive(true);
            Debug.Log("✅ Boss created: " + boss.name);
        }

    }

}