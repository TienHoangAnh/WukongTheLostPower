using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class PlayerSkillManager : MonoBehaviour
{
    private Dictionary<int, ISkill> unlockedSkills = new();
    private PlayerMovementContext context;

    [System.Serializable]
    public class SkillEntry
    {
        public string id;
        public Object behaviour;
    }

    [Header("Skill Catalog (Inspector)")]
    [Tooltip("Map skillId -> behaviour (ISkill). Kéo sẵn các skill vào đây.")]
    [SerializeField] private List<SkillEntry> skillCatalog = new();

    private readonly Dictionary<string, ISkill> idToSkill = new();

    private static readonly int[] ValidKeys = { 1, 2, 3, 4, 5 };

    [Header("Autosave")]
    [SerializeField] private float autosaveDebounce = 1.25f;
    private Coroutine saveCo;

    void Start()
    {
        context = GetComponent<PlayerMovementContext>();

        idToSkill.Clear();
        foreach (var e in skillCatalog)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id) || e.behaviour == null) continue;

            // behaviour có thể là ScriptableObject hoặc MonoBehaviour implement ISkill
            var skill = e.behaviour as ISkill;
            if (skill != null && !idToSkill.ContainsKey(e.id))
                idToSkill.Add(e.id, skill);
        }

        if (SaveRuntime.Current == null)
            SaveRuntime.Current = new SaveSlotDTO { currentMap = 1, player = new PlayerStateDTO() };
        if (SaveRuntime.Current.skillsUnlocked == null)
            SaveRuntime.Current.skillsUnlocked = new List<string>();

        unlockedSkills.Clear();
        int bindIndex = 0;
        foreach (var skillId in SaveRuntime.Current.skillsUnlocked)
        {
            if (!idToSkill.TryGetValue(skillId, out var skill)) continue;
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
        HandleKeyUse(1, KeyCode.Alpha1);
        HandleKeyUse(2, KeyCode.Alpha2);
        HandleKeyUse(3, KeyCode.Alpha3);
        HandleKeyUse(4, KeyCode.Alpha4);
        HandleKeyUse(5, KeyCode.Alpha5);
    }

    /// <summary>
    /// Xử lý nhấn phím kích hoạt skill: nếu có thì dùng, nếu chưa mở thì log ra.
    /// </summary>
    private void HandleKeyUse(int key, KeyCode keyCode)
    {
        if (!Input.GetKeyDown(keyCode))
            return;

        if (unlockedSkills.TryGetValue(key, out var skill) && skill != null)
        {
            skill.Use(context);
        }
        else
        {
            Debug.Log($"[PlayerSkillManager] Kỹ năng phím [{key}] chưa được mở khóa.");
        }
    }

    public void UnlockSkillById(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId)) return;

        if (!idToSkill.TryGetValue(skillId, out var skill))
        {
            Debug.LogWarning($"[PlayerSkillManager] Skill id '{skillId}' chưa có trong catalog.");
            return;
        }

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
            Debug.Log($"[PlayerSkillManager] Đã mở khóa '{skillId}' nhưng không còn phím trống (1..5). Dùng AssignSkillToKey để gán tay.");
        }
        else
        {
            unlockedSkills[freeKey] = skill;
            Debug.Log($"🔓 Đã mở khóa kỹ năng [{skill.GetName()}] và gán phím [{freeKey}]");
        }

        DebouncedSave();
    }

    public void AssignSkillToKey(int key, string skillId)
    {
        if (System.Array.IndexOf(ValidKeys, key) < 0)
        {
            Debug.LogWarning($"[PlayerSkillManager] Key {key} không hợp lệ. Chỉ hỗ trợ 1..5.");
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

    public List<string> GetUnlockedIds()
    {
        var ids = new List<string>();
        foreach (var kv in unlockedSkills)
        {
            var skill = kv.Value;
            if (skill == null) continue;
            string id = FindIdByInstance(skill);
            if (!string.IsNullOrEmpty(id) && !ids.Contains(id)) ids.Add(id);
        }
        if (ids.Count == 0 && SaveRuntime.Current != null && SaveRuntime.Current.skillsUnlocked != null)
        {
            foreach (var s in SaveRuntime.Current.skillsUnlocked)
                if (!ids.Contains(s)) ids.Add(s);
        }
        return ids;
    }

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
