using UnityEngine;
using System.Collections.Generic;

public class EnemySpawnController : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int enemyCount = 12;

    [Header("Chế độ spawn")]
    public bool useManualPoints = true;
    public Transform spawnPointsRoot;   // chứa các Empty làm điểm spawn thủ công

    [Header("Auto spawn nếu không dùng manual")]
    public Vector2 areaSize = new Vector2(200, 200);
    public int seed = 999;
    public Transform autoSpawnersRoot; // parent để chứa spawners auto

    [ContextMenu("Clear & Build Spawners")]
    public void ClearAndBuildSpawners()
    {
        if (useManualPoints)
        {
            BuildFromManual();
        }
        else
        {
            BuildAuto();
        }
    }

    void BuildFromManual()
    {
        if (!spawnPointsRoot)
        {
            Debug.LogWarning("[EnemySpawnController] spawnPointsRoot is null.");
            return;
        }
        // tạo/refresh spawner trên từng point, tối đa enemyCount
        int created = 0;
        foreach (Transform p in spawnPointsRoot)
        {
            if (created >= enemyCount) break;
            var sp = EnsureSpawner(p.gameObject);
            sp.enemyPrefab = enemyPrefab;
            sp.spawnOnStart = true;
            created++;
        }

        // nếu điểm thủ công ít hơn enemyCount, cảnh báo
        if (created < enemyCount)
        {
            Debug.LogWarning($"[EnemySpawnController] Points < enemyCount ({created} / {enemyCount}).");
        }
    }

    void BuildAuto()
    {
        Random.InitState(seed);
        // clear cũ
        if (!autoSpawnersRoot) autoSpawnersRoot = NewRoot("AutoSpawners");
        ClearChildren(autoSpawnersRoot);

        for (int i = 0; i < enemyCount; i++)
        {
            var go = new GameObject($"EnemySpawner_{i}");
            go.transform.SetParent(autoSpawnersRoot, false);
            go.transform.position = RandPosOnMap();

            var sp = go.AddComponent<EnemySpawner>();
            sp.enemyPrefab = enemyPrefab;
            sp.spawnOnStart = true;
        }
    }

    EnemySpawner EnsureSpawner(GameObject host)
    {
        var sp = host.GetComponent<EnemySpawner>();
        if (!sp) sp = host.AddComponent<EnemySpawner>();
        return sp;
    }

    Vector3 RandPosOnMap()
    {
        float x = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        float z = Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);
        return new Vector3(x, 0, z);
    }

    Transform NewRoot(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        return go.transform;
    }

    void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(t.GetChild(i).gameObject);
#else
            Destroy(t.GetChild(i).gameObject);
#endif
        }
    }
}
