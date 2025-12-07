using UnityEngine;

public class HybridBossAnimationManager : MonoBehaviour, IEnemyAnimationManager
{
    public Animator animator;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) Debug.LogWarning("[HybridBossAnimationManager] Animator not assigned on HybridBoss.");
    }

    public void PlayStanding()
    {
        if (animator == null) return;
        animator.SetTrigger("Standing");
    }

    public void PlayScream()
    {
        if (animator == null) return;
        animator.SetTrigger("Scream");
    }

    public void PlayChasePlayer()
    {
        if (animator == null) return;
        animator.SetBool("IsChasing", true);
    }

    public void StopChasePlayer()
    {
        if (animator == null) return;
        animator.SetBool("IsChasing", false);
    }

    public void Play360do()
    {
        if (animator == null) return;
        animator.SetTrigger("360do");
    }

    public void PlayTakeDamage()
    {
        if (animator == null) return;
        animator.SetTrigger("TakeDamage");
    }

    public void PlayKnockFallingBack()
    {
        if (animator == null) return;
        animator.SetTrigger("KnockFall");
    }

    public void PlayStandUp()
    {
        if (animator == null) return;
        animator.SetTrigger("StandUp");
    }

    public void PlayBreakLookAround()
    {
        if (animator == null) return;
        animator.SetTrigger("BreakLookAround");
    }

    public void PlayDie()
    {
        if (animator == null) return;
        animator.SetTrigger("Die");
    }

    public void UpdateHealth(float health)
    {
        if (animator == null) return;
        animator.SetFloat("Health", health);
    }

    public void PlayIdleSit()
    {
        if (animator == null) return;
        animator.SetTrigger("Idle_enemySit");
    }
}