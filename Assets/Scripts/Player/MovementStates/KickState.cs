using UnityEngine;

public class KickState : IMovementState
{
    public void EnterState(PlayerMovementContext context)
    {
        if (context.animator != null)
            context.animator.SetTrigger("Kick");

        // Gây sát thương trong phạm vi nhỏ trước mặt
        Vector3 origin = context.transform.position + context.transform.forward * 1f;
        float range = 1.5f;
        Collider[] hits = Physics.OverlapSphere(origin, range, context.enemyLayer);
        foreach (var hit in hits)
        {
            ICharacter target = hit.GetComponent<ICharacter>();
            if (target != null)
            {
                target.TakeDamage(10f); // damage tạm thời
                Debug.Log($"🥋 Kick gây 10 dmg lên {hit.name}");
            }
        }
    }

    public void UpdateState(PlayerMovementContext context) { }
}