using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Skills/InfernoBurstSkill")]
public class InfernoBurstSkill : ScriptableObject, ISkill
{
    public float radius = 4f;
    public float baseDamage = 60f;
    public float burnDamage = 10f;
    public float burnDuration = 3f;
    public float cooldown = 10f;
    private float _lastUsed = -999f;

    public void Use(PlayerMovementContext context)
    {
        if (Time.time < _lastUsed + cooldown)
        {
            Debug.Log("⏳ Inferno Burst cooldown!");
            return;
        }

        Collider[] hits = Physics.OverlapSphere(context.transform.position, radius, LayerMask.GetMask("Enemy"));
        foreach (var h in hits)
        {
            var enemy = h.GetComponent<ICharacter>();
            if (enemy != null)
            {
                enemy.TakeDamage(baseDamage);
                context.StartCoroutine(ApplyBurn(enemy));
            }
        }

        Debug.Log($"🔥 Inferno Burst gây {baseDamage} + Burn {burnDamage}/s trong {burnDuration}s");
        _lastUsed = Time.time;
    }

    IEnumerator ApplyBurn(ICharacter target)
    {
        for (float t = 0; t < burnDuration; t += 1f)
        {
            target.TakeDamage(burnDamage);
            yield return new WaitForSeconds(1f);
        }
    }

    public string GetName() => "Inferno Burst";
}
