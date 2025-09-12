using System.Collections.Generic;
using UnityEngine;
using System.Collections;

// NOTE: Không cần using Firebase gì cả. Chỉ SaveRuntime + CloudSaveManager (local/cloud-agnostic).

public class PlayerSkillManager : MonoBehaviour
{
    private Dictionary<int, ISkill> unlockedSkills = new();
    private PlayerMovementContext context;

    // ==================== PATCH: CATALOG & SAVE ====================
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

        // PATCH: build idToSkill map
        idToSkill.Clear();
        foreach (var e in skillCatalog)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id) || e.behaviour == null) continue;
            var skill = e.behaviour as ISkill;
            if (skill != null && !idToSkill.ContainsKey(e.id))
                idToSkill.Add(e.id, skill);
        }

        // PATCH: đảm bảo SaveRuntime tồn tại
        if (SaveRuntime.Current == null)
            SaveRuntime.Current = new SaveSlotDTO { chapterIndex = 1, player = new PlayerStateDTO() };
        if (SaveRuntime.Current.learnedSkills == null)
            SaveRuntime.Current.learnedSkills = new List<string>();

        // PATCH: khôi phục skill đã mở từ save & auto-assign vào 1..3 theo thứ tự đã lưu
        unlockedSkills.Clear();
        int bindIndex = 0;
        foreach (var skillId in SaveRuntime.Current.learnedSkills)
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

        // Mở rộng thêm nếu cần...
    }

    // ==================== CŨ (giữ nguyên API) + PATCH lưu ====================
    public void UnlockSkill(int key, ISkill skill)
    {
        if (skill == null) return;

        if (!unlockedSkills.ContainsKey(key))
        {
            unlockedSkills.Add(key, skill);
            Debug.Log($"🔓 Đã mở khóa kỹ năng [{skill.GetName()}] tại phím [{key}]");

            // PATCH: cố gắng tìm skillId từ catalog để lưu
            string skillId = FindIdByInstance(skill);
            if (!string.IsNullOrEmpty(skillId))
            {
                EnsureSaveLists();
                if (!SaveRuntime.Current.learnedSkills.Contains(skillId))
                    SaveRuntime.Current.learnedSkills.Add(skillId);
                DebouncedSave();
            }
            else
            {
                Debug.LogWarning($"[PlayerSkillManager] Không tìm thấy skillId trong catalog cho skill {skill.GetName()}. Hãy thêm vào skillCatalog.");
            }
        }
    }

    // ==================== PATCH: API theo skillId (ổn định cho save) ====================

    /// <summary>Mở khóa skill theo id (sẽ tự bind vào phím 1..3 còn trống).</summary>
    public void UnlockSkillById(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId)) return;

        if (!idToSkill.TryGetValue(skillId, out var skill))
        {
            Debug.LogWarning($"[PlayerSkillManager] Skill id '{skillId}' chưa có trong catalog.");
            return;
        }

        // nếu đã có trong save thì không thêm nữa
        EnsureSaveLists();
        if (!SaveRuntime.Current.learnedSkills.Contains(skillId))
            SaveRuntime.Current.learnedSkills.Add(skillId);

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
        if (!SaveRuntime.Current.learnedSkills.Contains(skillId))
            SaveRuntime.Current.learnedSkills.Add(skillId);

        DebouncedSave();
    }

    // ==================== PATCH: helpers ====================

    private void EnsureSaveLists()
    {
        if (SaveRuntime.Current == null)
            SaveRuntime.Current = new SaveSlotDTO { chapterIndex = 1, player = new PlayerStateDTO() };
        if (SaveRuntime.Current.learnedSkills == null)
            SaveRuntime.Current.learnedSkills = new List<string>();
    }

    private string FindIdByInstance(ISkill skill)
    {
        foreach (var e in skillCatalog)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id) || e.behaviour == null) continue;
            if (ReferenceEquals(e.behaviour, skill)) return e.id;
            // trường hợp behaviour là wrapper, có thể so sánh bằng GetName nếu bạn muốn:
            // if ((e.behaviour as ISkill)?.GetName() == skill.GetName()) return e.id;
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
