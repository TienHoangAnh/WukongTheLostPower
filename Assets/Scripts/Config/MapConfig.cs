using UnityEngine;
using System.Collections.Generic;

public enum Element { Kim, Moc, Thuy, Hoa, Tho }

[CreateAssetMenu(menuName = "Wukong/MapConfig")]
public class MapConfig : ScriptableObject
{
    public Element element;
    public GameObject enemyPrefab;
    public int enemyCount = 12;
    public bool useUnityTerrain = true;
    public Vector2 size = new Vector2(200, 200);
    public float heightScale = 12f;
    public int seed = 12345;
    public List<GameObject> propPrefabs;
    public int propCount = 80;
    public bool enableHazard = false;
    public GameObject hazardPrefab;
    public int hazardCount = 6;
    public Color ambientColor = new Color(0.2f, 0.2f, 0.2f);
    public float sunIntensity = 1.0f;
    public Color sunColor = Color.white;
    public Material skybox;
    public AudioClip bgm;
    public AudioClip ambientLoop;
    public Transform playerSpawn;
    public List<Transform> enemySpawnHints;
}
