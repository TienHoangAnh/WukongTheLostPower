using UnityEngine;

public class EnemyAnimationManager : MonoBehaviour
{
    public Animator animator;
    public EnemyAIContext enemyAIContext;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (enemyAIContext == null)
            enemyAIContext = GetComponent<EnemyAIContext>();
    }

    public void PlayStanding()
    {
        animator.SetTrigger("Standing");
    }

    public void PlayScream()
    {
        animator.SetTrigger("Scream");
    }

    public void PlayChasePlayer()
    {
        animator.SetBool("IsChasing", true);
    }

    public void StopChasePlayer()
    {
        animator.SetBool("IsChasing", false);
    }

    public void Play360do()
    {
        animator.SetTrigger("360do");
    }

    public void PlayTakeDamage()
    {
        animator.SetTrigger("TakeDamage");
    }

    public void PlayKnockFallingBack()
    {
        animator.SetTrigger("KnockFallingBack");
    }

    public void PlayStandUp()
    {
        animator.SetTrigger("StandUp");
    }

    public void PlayBreakLookAround()
    {
        animator.SetTrigger("BreakLookAround");
    }

    public void PlayDie()
    {
        animator.SetTrigger("Die");
    }

    public void UpdateHealth(float health)
    {
        animator.SetFloat("Health", health);
    }
}