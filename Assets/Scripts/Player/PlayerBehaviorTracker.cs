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

        // Restore counts from SaveRuntime if available
        if (SaveRuntime.Current != null)
        {
            var dto = SaveRuntime.Current;
            if (dto != null)
            {
                meleeCount = dto.meleeCount;
                rangedCount = dto.rangedCount;
            }
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

        // keep runtime DTO in sync so SaveRuntime has latest values
        if (SaveRuntime.Current != null)
            SaveRuntime.Current.meleeCount = meleeCount;
    }

    public void RecordRangedAttack()
    {
        rangedCount++;
        Debug.Log("🎯 Long range attack record:" + rangedCount);

        if (SaveRuntime.Current != null)
            SaveRuntime.Current.rangedCount = rangedCount;
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

        // Also persist counts into SaveRuntime so they are saved next time SaveRuntime is written
        if (SaveRuntime.Current != null)
        {
            SaveRuntime.Current.meleeCount = meleeCount;
            SaveRuntime.Current.rangedCount = rangedCount;
        }
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.SetString("Playstyle", GetPlaystyle());

        if (SaveRuntime.Current != null)
        {
            SaveRuntime.Current.meleeCount = meleeCount;
            SaveRuntime.Current.rangedCount = rangedCount;
        }
    }
}
