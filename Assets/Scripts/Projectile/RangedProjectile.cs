using UnityEngine;

public class RangedProjectile : MonoBehaviour
{
    private Transform target;
    public float speed = 5f;
    public float damage = 15f;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.Rotate(Vector3.up * 180f * Time.deltaTime); // hiệu ứng quay

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            ICharacter enemy = target.GetComponent<ICharacter>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"🔫 Đánh xa gây {damage} sát thương lên {target.name}");
            }
            Destroy(gameObject);
        }
    }
}
