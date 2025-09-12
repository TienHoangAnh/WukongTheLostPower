using UnityEngine;

public class PlayerBehaviorTracker : MonoBehaviour
{
    public int meleeCount = 0;
    public int rangedCount = 0;

    //DontDestroyOnLoad để tồn tại qua scene
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void RecordMeleeAttack()
    {
        meleeCount++;
        Debug.Log("🧨 Ghi nhận đòn cận chiến: " + meleeCount);
    }

    public void RecordRangedAttack()
    {
        rangedCount++;
        Debug.Log("🎯 Ghi nhận đòn tầm xa: " + rangedCount);
    }

    // Bạn có thể thêm hàm để phân tích xu hướng người chơi
    public string GetPlaystyle()
    {
        if (meleeCount > rangedCount * 1.5f) return "Melee";
        if (rangedCount > meleeCount * 1.5f) return "Ranged";
        return "Balanced";
    }

    void OnDestroy()
    {
        PlayerPrefs.SetString("Playstyle", GetPlaystyle()); // lưu vào PlayerPrefs để chuyển scene
    }

}
