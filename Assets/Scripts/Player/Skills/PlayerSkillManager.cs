using System.Collections.Generic;
using UnityEngine;
using System.Collections;

// NOTE: Không cần using Firebase gì cả. Chỉ SaveRuntime + CloudSaveManager (local/cloud-agnostic).
// Để mở khóa skill, luôn dùng UnlockSkillById("heal") với id đã có trong skillCatalog Inspector.

public class PlayerSkillManager : MonoBehaviour
{
    private Dictionary<int, ISkill> unlockedSkills = new();
    private PlayerMovementContext context;

    [System.Serializable]
    public class SkillEntry
    {
        public string id;                 // id ổn định để lưu vào save, ví dụ: "dash_slash"
        public MonoBehaviour behaviour;   // component triển khai ISkill (kéo vào Inspector)
    }

    [Header("Skill Catalog (Inspector)")]
    [Tooltip("Map skillId -> behaviour (ISkill). Kéo sẵn các skill vào đây.")]
    [SerializeField] private List<SkillEntry> skillCatalog = new();

    // skillId -> ISkill (runtime)
    private readonly Dictionary<string, ISkill> idToSkill = new();

    // phím hợp lệ để bind skill (1..3)
    private static readonly int[] ValidKeys = { 1, 2, 3 };

    [Header("Autosave")]
    [SerializeField] private float autosaveDebounce = 1.25f;
    private Coroutine saveCo;

    void Start()
    {
        context = GetComponent<PlayerMovementContext>();

        // Build idToSkill map
        idToSkill.Clear();
        foreach (var e in skillCatalog)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id) || e.behaviour == null) continue;
            var skill = e.behaviour as ISkill;
            if (skill != null && !idToSkill.ContainsKey(e.id))
                idToSkill.Add(e.id, skill);
        }

        // Đảm bảo SaveRuntime tồn tại (MODEL MỚI)
        if (SaveRuntime.Current == null)
            SaveRuntime.Current = new SaveSlotDTO { currentMap = 1, player = new PlayerStateDTO() };
        if (SaveRuntime.Current.skillsUnlocked == null)
            SaveRuntime.Current.skillsUnlocked = new List<string>();

        // Khôi phục skill đã mở từ save & auto-assign vào 1..3 theo thứ tự đã lưu
        unlockedSkills.Clear();
        int bindIndex = 0;
        foreach (var skillId in SaveRuntime.Current.skillsUnlocked)
        {
            if (!idToSkill.TryGetValue(skillId, out var skill)) continue;
            // gán lần lượt vào 1..3 nếu còn trống
            while (bindIndex < ValidKeys.Length && unlockedSkills.ContainsKey(ValidKeys[bindIndex])) bindIndex++;
            if (bindIndex < ValidKeys.Length)
            {
                unlockedSkills[ValidKeys[bindIndex]] = skill;
                bindIndex++;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && unlockedSkills.ContainsKey(1))
            unlockedSkills[1].Use(context);

        if (Input.GetKeyDown(KeyCode.Alpha2) && unlockedSkills.ContainsKey(2))
            unlockedSkills[2].Use(context);

        if (Input.GetKeyDown(KeyCode.Alpha3) && unlockedSkills.ContainsKey(3))
            unlockedSkills[3].Use(context);
    }

    // Luôn unlock skill bằng id, không tạo instance mới
    public void UnlockSkillById(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId)) return;

        if (!idToSkill.TryGetValue(skillId, out var skill))
        {
            Debug.LogWarning($"[PlayerSkillManager] Skill id '{skillId}' chưa có trong catalog.");
            return;
        }

        // Nếu player đang chết/đã chết thì không mở khóa
        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var ps = player.GetComponent<PlayerStats>();
            if (ps != null && ps.currentHealth <= 0f)
            {
                Debug.Log("[PlayerSkillManager] Player is dead - skipping unlock.");
                return;
            }
        }

        EnsureSaveLists();
        if (!SaveRuntime.Current.skillsUnlocked.Contains(skillId))
            SaveRuntime.Current.skillsUnlocked.Add(skillId);

        // tìm phím trống (1..3)
        int freeKey = -1;
        foreach (var k in ValidKeys)
        {
            if (!unlockedSkills.ContainsKey(k))
            {
                freeKey = k; break;
            }
        }
        if (freeKey == -1)
        {
            // nếu không còn phím trống, không override tự động – chỉ log
            Debug.Log($"[PlayerSkillManager] Đã mở khóa '{skillId}' nhưng không còn phím trống (1..3). Dùng AssignSkillToKey để gán tay.");
        }
        else
        {
            unlockedSkills[freeKey] = skill;
            Debug.Log($"🔓 Đã mở khóa kỹ năng [{skill.GetName()}] và gán phím [{freeKey}]");
        }

        DebouncedSave();
    }

    /// <summary>Gán 1 skill theo id vào phím chỉ định (1..3), override nếu đã có.</summary>
    public void AssignSkillToKey(int key, string skillId)
    {
        if (System.Array.IndexOf(ValidKeys, key) < 0)
        {
            Debug.LogWarning($"[PlayerSkillManager] Key {key} không hợp lệ. Chỉ hỗ trợ 1..3.");
            return;
        }

        if (!idToSkill.TryGetValue(skillId, out var skill))
        {
            Debug.LogWarning($"[PlayerSkillManager] Skill id '{skillId}' chưa có trong catalog.");
            return;
        }

        unlockedSkills[key] = skill;
        Debug.Log($"[PlayerSkillManager] Gán '{skillId}' vào phím [{key}]");

        EnsureSaveLists();
        if (!SaveRuntime.Current.skillsUnlocked.Contains(skillId))
            SaveRuntime.Current.skillsUnlocked.Add(skillId);

        DebouncedSave();
    }

    // ==================== Helpers ====================

    private void EnsureSaveLists()
    {
        if (SaveRuntime.Current == null)
            SaveRuntime.Current = new SaveSlotDTO { currentMap = 1, player = new PlayerStateDTO() };
        if (SaveRuntime.Current.skillsUnlocked == null)
            SaveRuntime.Current.skillsUnlocked = new List<string>();
    }

    private string FindIdByInstance(ISkill skill)
    {
        foreach (var e in skillCatalog)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id) || e.behaviour == null) continue;
            if (ReferenceEquals(e.behaviour, skill)) return e.id;
        }
        return null;
    }

    private void DebouncedSave()
    {
        if (saveCo != null) StopCoroutine(saveCo);
        saveCo = StartCoroutine(CoDebouncedSave());
    }

    private IEnumerator CoDebouncedSave()
    {
        yield return new WaitForSeconds(autosaveDebounce > 0 ? autosaveDebounce : 1.0f);
        if (SaveRuntime.Current != null)
            yield return CloudSaveManager.SaveNow(SaveRuntime.Current).AsIEnumerator();
    }
}
