using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{

    private readonly Dictionary<int, ISkill> unlockedSkills = new();

    private PlayerMovementContext context;

    [System.Serializable]
    public class SkillEntry
    {
        public string id;          // Unique skill id (e.g. "heal")
        public Object behaviour;   // Object implementing ISkill (ScriptableObject or MonoBehaviour)
    }

    [Header("Skill Catalog (Inspector)")]
    [Tooltip("Maps skillId to behaviour (ISkill). Drag skill implementations here.")]
    [SerializeField]
    private List<SkillEntry> skillCatalog = new();

    private readonly Dictionary<string, ISkill> idToSkill = new();

    private static readonly int[] ValidKeys = { 1, 2, 3, 4, 5 };

    [Header("Autosave")]
    [Tooltip("Delay before autosaving after unlocking or assigning a skill.")]
    [SerializeField]
    private float autosaveDebounce = 1.25f;

    private Coroutine saveCo;

    private void Start()
    {
        context = GetComponent<PlayerMovementContext>();

        // Build id -> skill lookup from inspector catalog
        idToSkill.Clear();
        foreach (var entry in skillCatalog)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.behaviour == null)
                continue;

            var skill = entry.behaviour as ISkill;
            if (skill != null && !idToSkill.ContainsKey(entry.id))
                idToSkill.Add(entry.id, skill);
        }

        // Ensure save runtime exists
        if (SaveRuntime.Current == null)
            SaveRuntime.Current = new SaveSlotDTO { currentMap = 1, player = new PlayerStateDTO() };
        if (SaveRuntime.Current.skillsUnlocked == null)
            SaveRuntime.Current.skillsUnlocked = new List<string>();

        // Restore unlocked skills and cooldowns from save
        ReloadFromSaveRuntime();
    }

    private void Update()
    {
        HandleKeyUse(1, KeyCode.Alpha1);
        HandleKeyUse(2, KeyCode.Alpha2);
        HandleKeyUse(3, KeyCode.Alpha3);
        HandleKeyUse(4, KeyCode.Alpha4);
        HandleKeyUse(5, KeyCode.Alpha5);
    }

    public void ReloadFromSaveRuntime()
    {
        // Ensure structures exist
        if (SaveRuntime.Current == null)
        {
            SaveRuntime.Current = new SaveSlotDTO { currentMap = 1, player = new PlayerStateDTO() };
        }
        if (SaveRuntime.Current.skillsUnlocked == null)
            SaveRuntime.Current.skillsUnlocked = new List<string>();

        // Clear runtime mapping
        unlockedSkills.Clear();

        int bindIndex = 0;
        foreach (var skillId in SaveRuntime.Current.skillsUnlocked)
        {
            if (!idToSkill.TryGetValue(skillId, out var skill))
                continue;

            // Find next free key slot
            while (bindIndex < ValidKeys.Length && unlockedSkills.ContainsKey(ValidKeys[bindIndex]))
                bindIndex++;

            if (bindIndex < ValidKeys.Length)
            {
                unlockedSkills[ValidKeys[bindIndex]] = skill;
                bindIndex++;
            }
        }

        // Restore skill cooldowns (if save contains them)
        try
        {
            if (SaveRuntime.Current != null && SaveRuntime.Current.skillCooldowns != null)
            {
                foreach (var kv in SaveRuntime.Current.skillCooldowns)
                {
                    var id = kv.Key;
                    var remaining = kv.Value;
                    if (string.IsNullOrWhiteSpace(id)) continue;
                    if (idToSkill.TryGetValue(id, out var s))
                    {
                        if (s is ICooldownSkill cd)
                        {
                            cd.RestoreCooldown(remaining);
                            Debug.Log($"[PlayerSkillManager] Restored cooldown for {id} = {remaining}s");
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[PlayerSkillManager] Failed to restore skill cooldowns: {ex.Message}");
        }
    }

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
            Debug.Log($"[PlayerSkillManager] No skill is unlocked for key [{key}].");
        }
    }

    public void UnlockSkillById(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId))
            return;

        if (!idToSkill.TryGetValue(skillId, out var skill))
        {
            Debug.LogWarning($"[PlayerSkillManager] Skill id '{skillId}' is not present in the catalog.");
            return;
        }

        // Optional: do not unlock if the player is currently dead
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

        // Find first free key (1..5)
        int freeKey = -1;
        foreach (var k in ValidKeys)
        {
            if (!unlockedSkills.ContainsKey(k))
            {
                freeKey = k;
                break;
            }
        }

        if (freeKey == -1)
        {
            Debug.Log($"[PlayerSkillManager] Skill '{skillId}' unlocked but no free keys (1..5) are available. Use AssignSkillToKey to rebind manually.");
        }
        else
        {
            unlockedSkills[freeKey] = skill;
            Debug.Log($"[PlayerSkillManager] Unlocked skill [{skill.GetName()}] and assigned to key [{freeKey}].");
        }

        DebouncedSave();
    }

    public void AssignSkillToKey(int key, string skillId)
    {
        if (System.Array.IndexOf(ValidKeys, key) < 0)
        {
            Debug.LogWarning($"[PlayerSkillManager] Key {key} is not valid. Only keys 1..5 are supported.");
            return;
        }

        if (!idToSkill.TryGetValue(skillId, out var skill))
        {
            Debug.LogWarning($"[PlayerSkillManager] Skill id '{skillId}' is not present in the catalog.");
            return;
        }

        unlockedSkills[key] = skill;
        Debug.Log($"[PlayerSkillManager] Assigned skill '{skillId}' to key [{key}].");

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
            if (skill == null)
                continue;

            string id = FindIdByInstance(skill);
            if (!string.IsNullOrEmpty(id) && !ids.Contains(id))
                ids.Add(id);
        }

        // Fallback to raw save data if the runtime mapping is empty
        if (ids.Count == 0 && SaveRuntime.Current != null && SaveRuntime.Current.skillsUnlocked != null)
        {
            foreach (var s in SaveRuntime.Current.skillsUnlocked)
            {
                if (!ids.Contains(s))
                    ids.Add(s);
            }
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
        foreach (var entry in skillCatalog)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.behaviour == null)
                continue;

            if (ReferenceEquals(entry.behaviour, skill))
                return entry.id;
        }

        return null;
    }

    private void DebouncedSave()
    {
        if (saveCo != null)
            StopCoroutine(saveCo);

        saveCo = StartCoroutine(CoDebouncedSave());
    }

    private IEnumerator CoDebouncedSave()
    {
        float delay = autosaveDebounce > 0 ? autosaveDebounce : 1.0f;
        yield return new WaitForSeconds(delay);

        if (SaveRuntime.Current != null)
            yield return CloudSaveManager.SaveNow(SaveRuntime.Current).AsIEnumerator();
    }
}
