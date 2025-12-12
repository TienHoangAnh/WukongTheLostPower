//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(PlayerStats))]
//public class PlayerComboAttack : MonoBehaviour
//{
//    [Header("Combo Settings")]
//    public ComboData comboData;
//    public Transform attackPoint;
//    public LayerMask enemyLayer = ~0;
//    public float attackRadius = 1.5f;

//    private int comboStep = 0;
//    private bool isAttacking = false;
//    private bool inputBuffered = false;
//    private float attackStartTime = -999f;

//    // Cooldown per combo step
//    private float[] nextReadyTimes;

//    private PlayerStats stats;
//    private Animator animator;

//    void Awake()
//    {
//        stats = GetComponent<PlayerStats>();
//        animator = GetComponent<Animator>();
//    }

//    void Start()
//    {
//        if (comboData == null || comboData.comboSteps == null || comboData.comboSteps.Count == 0)
//        {
//            Debug.LogError("[Combo] comboData chưa được gán!");
//            enabled = false;
//            return;
//        }

//        nextReadyTimes = new float[comboData.comboSteps.Count];
//    }

//    void Update()
//    {
//        if (isAttacking)
//        {
//            HandleComboWindow();
//        }
//        else
//        {
//            if (inputBuffered)
//            {
//                inputBuffered = false;
//                StartCombo();
//            }
//        }
//    }

//    // Call this from input system (e.g. key handlers Q/E/R or UI buttons)
//    public void BufferAttackInput()
//    {
//        inputBuffered = true;
//    }
//    void HandleComboWindow()
//    {
//        var currentStep = comboData.comboSteps[comboStep - 1];
//        float elapsed = Time.time - attackStartTime;

//        // Nếu nhấn trong thời gian inputWindow => qua bước kế tiếp
//        if (inputBuffered && elapsed <= currentStep.inputWindow)
//        {
//            inputBuffered = false;
//            ProceedToNextCombo();
//        }

//        // Nếu quá duration => kết thúc combo
//        if (elapsed > currentStep.duration)
//        {
//            isAttacking = false;
//            comboStep = 0;
//            Debug.Log("[Combo] Hết chuỗi combo.");
//        }
//    }

//    void StartCombo()
//    {
//        comboStep = 1;
//        ExecuteComboStep();
//    }

//    void ProceedToNextCombo()
//    {
//        comboStep++;
//        if (comboStep > comboData.comboSteps.Count)
//            comboStep = 1;

//        ExecuteComboStep();
//    }

//    void ExecuteComboStep()
//    {
//        int index = comboStep - 1;
//        var step = comboData.comboSteps[index];
//        float now = Time.time;

//        // Cooldown check
//        if (now < nextReadyTimes[index])
//        {
//            Debug.Log($"[Combo] ⏳ Chiêu {comboStep} đang hồi chiêu, còn {nextReadyTimes[index] - now:0.00}s");
//            return;
//        }

//        // Check stamina
//        if (!stats.UseStamina(step.staminaCost))
//        {
//            Debug.Log($"[Combo] ❌ Không đủ stamina ({stats.currentStamina:0.0}/{stats.maxStamina:0.0}) để dùng Chiêu {comboStep}");
//            return;
//        }

//        // Animation (nếu có)
//        if (animator && !string.IsNullOrEmpty(step.animationName))
//            animator.SetTrigger(step.animationName);

//        // Bắt đầu đòn
//        isAttacking = true;
//        attackStartTime = now;

//        // Damage
//        float finalDamage = step.damage > 0 ? step.damage : stats.baseDamage * (1 + step.bonusPercent / 100f);
//        int hitCount = 0;

//        // OverlapSphere hit
//        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRadius, enemyLayer);
//        foreach (var h in hits)
//        {
//            var target = h.GetComponent<ICharacter>();
//            if (target != null)
//            {
//                target.TakeDamage(finalDamage);
//                hitCount++;
//            }
//        }

//        // Set cooldown
//        nextReadyTimes[index] = now + step.cooldown;

//        // Debug chi tiết
//        Debug.Log(
//            $"💥 Chiêu {comboStep} - {step.skillName} | Dmg: {finalDamage} | " +
//            $"Hit: {hitCount} | Stamina dùng: {step.staminaCost} | " +
//            $"Còn lại: {stats.currentStamina:0.0}/{stats.maxStamina:0.0} | " +
//            $"CD: {step.cooldown:0.00}s | InputWindow: {step.inputWindow:0.00}s"
//        );
//    }

//    void OnDrawGizmosSelected()
//    {
//        if (attackPoint)
//        {
//            Gizmos.color = Color.red;
//            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
//        }
//    }
//}
