using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComboAttack : MonoBehaviour
{
    [Header("Combo Settings")]
    public ComboData comboData;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    private int comboStep = 0;
    private bool isAttacking = false;
    private bool inputBuffered = false;
    private float attackStartTime = -999f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            inputBuffered = true;
        }

        if (isAttacking)
        {
            HandleComboWindow();
        }
        else
        {
            if (inputBuffered)
            {
                inputBuffered = false;
                StartCombo();
            }
        }
    }

    void HandleComboWindow()
    {
        var currentStep = comboData.comboSteps[comboStep - 1];
        float elapsed = Time.time - attackStartTime;

        if (inputBuffered && elapsed <= currentStep.inputWindow)
        {
            inputBuffered = false;
            ProceedToNextCombo();
        }

        if (elapsed > currentStep.duration)
        {
            isAttacking = false;
            comboStep = 0;
        }
    }

    void StartCombo()
    {
        comboStep = 1;
        ExecuteComboStep();
    }

    void ProceedToNextCombo()
    {
        comboStep++;
        if (comboStep > comboData.comboSteps.Count)
            comboStep = 1;

        ExecuteComboStep();
    }

    void ExecuteComboStep()
    {
        isAttacking = true;
        attackStartTime = Time.time;

        AttackStep step = comboData.comboSteps[comboStep - 1];

        Debug.Log($"💥 Chiêu {comboStep} - Gây {step.damage} sát thương");

        // Attack logic
        Collider[] hits = Physics.OverlapSphere(attackPoint.position, 1.5f, enemyLayer);
        foreach (Collider enemy in hits)
        {
            ICharacter target = enemy.GetComponent<ICharacter>();
            if (target != null)
            {
                target.TakeDamage(step.damage);
                Debug.Log($"🎯 Đánh trúng {enemy.name} với {step.damage} sát thương");
            }
        }

        // Optional: animation, sfx, vfx here
        // Animator.SetTrigger("Combo" + comboStep);
        // AudioSource.PlayClipAtPoint(step.sfx, transform.position);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, 1.5f);
        }
    }
}
