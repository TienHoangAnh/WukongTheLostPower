using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RangedClone : MonoBehaviour
{
    [System.Serializable]
    public struct Config
    {
        public float damage;
        public float speed;
        public float turnRateDegPerSec;
        public float impactRadius;
        public float knockbackForce;
        public LayerMask enemyLayer;
        public LayerMask obstacleMask;
        public string enemyTag;
        public GameObject impactVFX;
        public AudioClip impactSFX;
    }

    private Config cfg;
    private Transform target;
    private Transform owner;

    [Header("Runtime")]
    [SerializeField] private float maxLifetime = 6f; // đủ để rise + lao + va chạm
    private float age = 0f;
    private bool detonated = false;

    public void Init(Config config) => cfg = config;

    public void SetTarget(Transform t, Transform ownerRef)
    {
        target = t;
        owner = ownerRef;
    }

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true; // dùng trigger để kiểm soát nổ
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age > maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // CHỜ TARGET: trước khi PlayerCombat gán target, clone đứng yên (hoặc có thể idle VFX)
        if (target == null) return;

        // Nếu target bị disable/chết → tự hủy (hoặc có thể chọn target mới tuỳ ý)
        if (!target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        // Homing quay dần theo target
        Vector3 toTarget = target.position - transform.position;
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desired,
                cfg.turnRateDegPerSec * Time.deltaTime
            );
        }

        // Tiến tới
        transform.position += transform.forward * cfg.speed * Time.deltaTime;

        // Nổ nếu đủ gần
        if (!detonated && toTarget.sqrMagnitude <= (cfg.impactRadius * cfg.impactRadius))
        {
            Detonate();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (detonated) return;

        // Chỉ quan tâm enemy layer
        if (((1 << other.gameObject.layer) & cfg.enemyLayer.value) == 0) return;

        // Nếu dùng tag lọc thêm
        if (!string.IsNullOrEmpty(cfg.enemyTag) && !other.CompareTag(cfg.enemyTag)) return;

        Detonate();
    }

    void Detonate()
    {
        detonated = true;

        // Gây damage/knockback trong bán kính nhỏ
        var cols = Physics.OverlapSphere(transform.position, cfg.impactRadius, cfg.enemyLayer);
        foreach (var c in cols)
        {
            var ichar = c.GetComponent<ICharacter>();
            if (ichar != null)
            {
                ichar.TakeDamage(cfg.damage);

                if (c.attachedRigidbody != null && cfg.knockbackForce > 0f)
                {
                    Vector3 dir = (c.transform.position - transform.position).normalized;
                    c.attachedRigidbody.AddForce(dir * cfg.knockbackForce, ForceMode.Impulse);
                }
            }
        }

        if (cfg.impactVFX) Instantiate(cfg.impactVFX, transform.position, Quaternion.identity);
        if (cfg.impactSFX) AudioSource.PlayClipAtPoint(cfg.impactSFX, transform.position, 0.9f);

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, cfg.impactRadius);
    }
#endif
}
