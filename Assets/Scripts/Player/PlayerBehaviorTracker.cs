using UnityEngine;

public class PlayerBehaviorTracker : MonoBehaviour
{
    public static PlayerBehaviorTracker Instance { get; private set; }

    public int meleeCount = 0;
    public int rangedCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        // Map Q/E/R to melee attacks
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R))
        {
            RecordMeleeAttack();
        }

        // J is ranged attack
        if (Input.GetKeyDown(KeyCode.J))
        {
            RecordRangedAttack();
        }
    }

    public void RecordMeleeAttack()
    {
        meleeCount++;
        Debug.Log("🧨 Melee attack recording: " + meleeCount);
    }

    public void RecordRangedAttack()
    {
        rangedCount++;
        Debug.Log("🎯 Long range attack record:" + rangedCount);
    }

    public string GetPlaystyle()
    {
        if (meleeCount > rangedCount * 1.5f) return "Melee";
        if (rangedCount > meleeCount * 1.5f) return "Ranged";
        return "Balanced";
    }

    void OnDestroy()
    {
        PlayerPrefs.SetString("Playstyle", GetPlaystyle()); 
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetString("Playstyle", GetPlaystyle());
    }
}
