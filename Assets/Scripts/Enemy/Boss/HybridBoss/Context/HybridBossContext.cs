using UnityEngine;

public class HybridBossContext : MonoBehaviour
{
    public HybridBossAnimationManager animationManager;
    public BossStats bossStats;
    public Transform player;

    void Start()
    {
        animationManager = GetComponent<HybridBossAnimationManager>();
        bossStats = GetComponent<BossStats>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (bossStats.currentHealth <= 0)
        {
            animationManager.PlayDie();
        }
        else if (bossStats.currentHealth <= bossStats.maxHealth / 2)
        {
            animationManager.PlayKnockFallingBack();
        }
        else if (Vector3.Distance(transform.position, player.position) <= 10f)
        {
            animationManager.PlayStanding();
            animationManager.PlayScream();
        }
        else
        {
            animationManager.PlayChasePlayer();
        }
    }
}