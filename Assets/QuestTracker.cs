//using UnityEngine;
//using UnityEngine.Events;

//public class QuestTracker : MonoBehaviour
//{
//    public static QuestTracker Instance;
//    public int totalEnemies;
//    public int killed;
//    public UnityEvent onAllCleared;
//    void Awake() { Instance = this; }
//    public void OnEnemyKilled()
//    {
//        killed++;
//        UI_MiniQuest.UpdateCounter(killed, totalEnemies);
//        if (killed >= totalEnemies) onAllCleared?.Invoke();
//    }
//}
