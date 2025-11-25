//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;
//using System.Collections.Generic;

//public class MapBuilder : MonoBehaviour
//{
//    public MapConfig config;
//    public Transform envRoot;
//    public Transform spawnsRoot;
//    public Transform hazardsRoot;
//    public Transform propsRoot;
//    public Transform rewardsRoot;

//    [Header("References")]
//    public Light sunLight;
//    public Volume postProcessVolume;
//    public AudioSource bgmSource;
//    public AudioSource ambientSource;

//    [ContextMenu("Build Map")]
//    public void BuildMap()
//    {
//        Random.InitState(config.seed);

//        BuildLighting();
//        BuildTerrainOrGround();
//        PlaceProps();
//        PlaceHazards();
//        PlaceSpawners();
//        SetupAudio();
//    }

//    void BuildLighting()
//    {
//        RenderSettings.ambientLight = config.ambientColor;
//        if (sunLight != null)
//        {
//            sunLight.intensity = config.sunIntensity;
//            sunLight.color = config.sunColor;
//        }
//        if (config.skybox) RenderSettings.skybox = config.skybox;
//        DynamicGI.UpdateEnvironment();
//    }

//    void BuildTerrainOrGround()
//    {
//        if (config.useUnityTerrain)
//        {
//            // Simple flat terrain with noise
//            var terrainGO = new GameObject("Terrain");
//            terrainGO.transform.SetParent(envRoot, false);
//            var td = new TerrainData
//            {
//                heightmapResolution = 257,
//                size = new Vector3(config.size.x, config.heightScale, config.size.y)
//            };
//            // (optional) height noise: just a gentle ripple
//            float[,] heights = new float[td.heightmapResolution, td.heightmapResolution];
//            for (int y = 0; y < td.heightmapResolution; y++)
//            {
//                for (int x = 0; x < td.heightmapResolution; x++)
//                {
//                    float nx = (float)x / td.heightmapResolution;
//                    float ny = (float)y / td.heightmapResolution;
//                    heights[y, x] = Mathf.PerlinNoise(nx * 2f + config.seed, ny * 2f) * 0.05f;
//                }
//            }
//            td.SetHeights(0, 0, heights);
//            var terrain = terrainGO.AddComponent<Terrain>();
//            terrain.terrainData = td;
//            terrainGO.AddComponent<TerrainCollider>().terrainData = td;
//        }
//        else
//        {
//            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
//            ground.name = "Ground";
//            ground.transform.SetParent(envRoot, false);
//            ground.transform.localScale = new Vector3(config.size.x / 10f, 1, config.size.y / 10f);
//        }
//    }

//    void PlaceProps()
//    {
//        if (config.propPrefabs == null || config.propPrefabs.Count == 0) return;
//        for (int i = 0; i < config.propCount; i++)
//        {
//            var pf = config.propPrefabs[Random.Range(0, config.propPrefabs.Count)];
//            Vector3 pos = RandPosOnMap();
//            var go = Instantiate(pf, pos, Quaternion.Euler(0, Random.Range(0, 360), 0), propsRoot);
//            float s = Random.Range(0.9f, 1.3f);
//            go.transform.localScale = Vector3.one * s;
//        }
//    }

//    void PlaceHazards()
//    {
//        if (!config.enableHazard || config.hazardPrefab == null) return;
//        for (int i = 0; i < config.hazardCount; i++)
//        {
//            var pos = RandPosOnMap();
//            Instantiate(config.hazardPrefab, pos, Quaternion.identity, hazardsRoot);
//        }
//    }

//    void PlaceSpawners()
//    {
//        int count = config.enemyCount;
//        var hints = (config.enemySpawnHints != null && config.enemySpawnHints.Count > 0);
//        for (int i = 0; i < count; i++)
//        {
//            Vector3 pos = hints ? config.enemySpawnHints[i % config.enemySpawnHints.Count].position
//                                : RandPosOnMap();
//            var sp = new GameObject($"EnemySpawner_{i}").AddComponent<EnemySpawner>();
//            sp.transform.SetParent(spawnsRoot, false);
//            sp.transform.position = pos;
//            sp.enemyPrefab = config.enemyPrefab;
//        }
//    }

//    Vector3 RandPosOnMap()
//    {
//        float x = Random.Range(-config.size.x * 0.5f, config.size.x * 0.5f);
//        float z = Random.Range(-config.size.y * 0.5f, config.size.y * 0.5f);
//        return new Vector3(x, 0, z);
//    }

//    void SetupAudio()
//    {
//        if (bgmSource && config.bgm) { bgmSource.clip = config.bgm; bgmSource.loop = true; bgmSource.Play(); }
//        if (ambientSource && config.ambientLoop) { ambientSource.clip = config.ambientLoop; ambientSource.loop = true; ambientSource.Play(); }
//    }
//}
