//using UnityEngine;
//using UnityEngine.UI;

//public class BossUIManager : MonoBehaviour
//{
//    public Slider healthSlider;
//    private BossStats bossStats;
//    private Transform player;
//    public Transform bossTransform;
//    public float showRange = 20f;

//    void Start()                
//    {
//        bossStats = FindFirstObjectByType<BossStats>();
//        if (bossStats == null)
//        {
//            Debug.LogError("❌ BossStats object not found!");
//            return;
//        }

//        if (healthSlider == null)
//        {
//            Debug.LogError("❌ HealthSlider reference is missing!");
//            return;
//        }

//        healthSlider.maxValue = bossStats.maxHealth;
//        player = GameObject.FindGameObjectWithTag("Player")?.transform;
//    }

//    void Update()
//    {
//        if (bossStats == null || healthSlider == null )
//            return;

//        healthSlider.value = bossStats.currentHealth;

//        if (player != null && bossTransform != null)
//        {
//            float dist = Vector3.Distance(player.position, bossTransform.position);
//        }
//    }
//}