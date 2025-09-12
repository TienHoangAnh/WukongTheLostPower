using UnityEngine;

public class BossBehaviorManager : MonoBehaviour
{
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;
    public Transform spawnPoint;

    void Start()
    {
        string style = PlayerPrefs.GetString("Playstyle", "Balanced");

        if (style == "Melee")
        {
            Instantiate(meleeEnemyPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("💢 Boss triệu hồi quái CẬN CHIẾN");
        }
        else if (style == "Ranged")
        {
            Instantiate(rangedEnemyPrefab, spawnPoint.position, Quaternion.identity);
            Debug.Log("💥 Boss triệu hồi quái TẦM XA");
        }
        else
        {
            Instantiate(meleeEnemyPrefab, spawnPoint.position, Quaternion.identity);
            Instantiate(rangedEnemyPrefab, spawnPoint.position + Vector3.right * 2f, Quaternion.identity);
            Debug.Log("🌀 Boss triệu hồi cả hai loại quái");
        }
    }
}
