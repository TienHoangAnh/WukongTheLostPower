using UnityEngine;
using System.Collections.Generic;

public class PropScatter : MonoBehaviour
{
    [Header("Khu vực map (X,Z) mét")]
    public Vector2 areaSize = new Vector2(200, 200);

    [Header("Prefabs")]
    public GameObject groundPrefab;      // nền đất (plane/mesh) - optional
    public List<GameObject> trees;       // cây
    public List<GameObject> grasses;     // cỏ bụi
    public List<GameObject> rocks;       // đá nhỏ
    public List<GameObject> boulders;    // tảng đá lớn

    [Header("Số lượng mỗi nhóm")]
    public int treeCount = 40;
    public int grassCount = 120;
    public int rockCount = 60;
    public int boulderCount = 15;

    [Header("Ngẫu nhiên")]
    public int seed = 1234;
    public Vector2 uniformScaleRange = new Vector2(0.9f, 1.3f);

    [Header("Parents (tự tạo nếu trống)")]
    public Transform groundRoot;
    public Transform treesRoot;
    public Transform grassesRoot;
    public Transform rocksRoot;
    public Transform bouldersRoot;

    [ContextMenu("Clear & Rebuild Props")]
    public void ClearAndRebuild()
    {
        Random.InitState(seed);
        EnsureRoots();

        // clear cũ
        ClearChildren(treesRoot);
        ClearChildren(grassesRoot);
        ClearChildren(rocksRoot);
        ClearChildren(bouldersRoot);
        ClearChildren(groundRoot);

        // nền
        if (groundPrefab != null)
        {
            var g = Instantiate(groundPrefab, Vector3.zero, Quaternion.identity, groundRoot);
            // nếu là Plane mặc định 10x10 -> scale theo areaSize
            var plane = g.GetComponent<MeshFilter>();
            if (plane != null)
            {
                g.transform.localScale = new Vector3(areaSize.x / 10f, 1, areaSize.y / 10f);
            }
        }

        // scatter từng nhóm
        ScatterGroup(trees, treeCount, treesRoot);
        ScatterGroup(grasses, grassCount, grassesRoot);
        ScatterGroup(rocks, rockCount, rocksRoot);
        ScatterGroup(boulders, boulderCount, bouldersRoot);
    }

    void EnsureRoots()
    {
        if (!groundRoot) groundRoot = NewRoot("Ground");
        if (!treesRoot) treesRoot = NewRoot("Props_Trees");
        if (!grassesRoot) grassesRoot = NewRoot("Props_Grasses");
        if (!rocksRoot) rocksRoot = NewRoot("Props_Rocks");
        if (!bouldersRoot) bouldersRoot = NewRoot("Props_Boulders");
    }

    Transform NewRoot(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        return go.transform;
    }

    void ClearChildren(Transform t)
    {
        if (!t) return;
        for (int i = t.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(t.GetChild(i).gameObject);
#else
            Destroy(t.GetChild(i).gameObject);
#endif
        }
    }

    void ScatterGroup(List<GameObject> prefabs, int count, Transform parent)
    {
        if (prefabs == null || prefabs.Count == 0 || count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            var pf = prefabs[Random.Range(0, prefabs.Count)];
            Vector3 pos = RandPosOnMap();
            var rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0f);
            var go = Instantiate(pf, pos, rot, parent);
            float s = Random.Range(uniformScaleRange.x, uniformScaleRange.y);
            go.transform.localScale = new Vector3(s, s, s);
        }
    }

    Vector3 RandPosOnMap()
    {
        float x = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        float z = Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);
        return new Vector3(x, 0, z);
    }
}
