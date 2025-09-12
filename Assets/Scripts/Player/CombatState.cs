// Scripts/Player/CombatState.cs
using UnityEngine;
using System.Collections;

public class CombatState : IMovementState
{
    int currentIndex = 0;
    bool inputReceived = false;
    Coroutine attackRoutine;

    public void EnterState(PlayerMovementContext context)
    {
        context.isAttacking = true;
        attackRoutine = context.StartCoroutine(AttackRoutine(context));
    }

    public void UpdateState(PlayerMovementContext context)
    {
        if (Input.GetMouseButtonDown(0))
            inputReceived = true;
    }

    public void ExitState(PlayerMovementContext context)
    {
        context.isAttacking = false;
        if (attackRoutine != null)
            context.StopCoroutine(attackRoutine);
    }
    IEnumerator AttackRoutine(PlayerMovementContext context)
    {
        ComboData combo = context.comboData;
        while (currentIndex < combo.comboSteps.Count)
        {
            inputReceived = false;
            AttackStep step = combo.comboSteps[currentIndex];

            Debug.Log($"[COMBO] Player thực hiện đòn số {currentIndex + 1} với {step.damage} sát thương");

            context.PerformAttack(step.damage);

            float timer = 0f;
            while (timer < step.inputWindow)
            {
                if (inputReceived) break;

                timer += Time.deltaTime;
                yield return null;
            }

            if (inputReceived)
            {
                currentIndex++;
                yield return new WaitForSeconds(step.duration);
            }
            else
            {
                break;
            }
        }

        Debug.Log($"[COMBO] Combo kết thúc ở đòn số {currentIndex + 1}/{combo.comboSteps.Count}");
        context.SwitchState(new WalkState());
    }
}
