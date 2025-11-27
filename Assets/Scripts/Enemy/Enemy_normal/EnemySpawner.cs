using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    [Tooltip("Spawn ngay khi Play")]
    public bool spawnOnStart = true;

    private GameObject alive;

    void Start()
    {
        if (spawnOnStart) Spawn();
    }

    public void Spawn()
    {
        if (!enemyPrefab)
        {
            Debug.LogWarning($"[{name}] Missing enemyPrefab");
            return;
        }
        if (alive != null) return;
        alive = Instantiate(enemyPrefab, transform.position, transform.rotation);
        // nếu dùng hệ thống đếm kill:
        var deathHook = alive.GetComponent<OnDeathNotify>() ?? alive.AddComponent<OnDeathNotify>();
        deathHook.onDeath += () => {
            alive = null;
            // ví dụ: QuestTracker.Instance?.OnEnemyKilled();
        };
    }
}

public class OnDeathNotify : MonoBehaviour
{
    public System.Action onDeath;
    public void NotifyDeath()
    {
        onDeath?.Invoke();
        Destroy(gameObject);
    }
}
