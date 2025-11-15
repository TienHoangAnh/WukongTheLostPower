using UnityEngine;

public static class SaveRuntime
{
    public static SaveSlotDTO Current { get; set; } = new SaveSlotDTO();

    public static void Apply(SaveSlotDTO dto)
    {
        if (dto == null) return;
        Current = dto;
    }

    public static void EnsureInitialized()
    {
        if (Current == null) Current = new SaveSlotDTO();
        if (Current.player == null) Current.player = new PlayerStateDTO();
        if (Current.inventory == null) Current.inventory = new InventorySnapshot();
        if (Current.skillsUnlocked == null) Current.skillsUnlocked = new System.Collections.Generic.List<string>();
        if (Current.deadEnemies == null) Current.deadEnemies = new System.Collections.Generic.List<string>();
        if (Current.bossesDefeated == null) Current.bossesDefeated = new System.Collections.Generic.List<string>();
        if (Current.worldFlags == null) Current.worldFlags = new System.Collections.Generic.Dictionary<string, bool>();
        if (Current.skillCooldowns == null) Current.skillCooldowns = new System.Collections.Generic.Dictionary<string, float>();
    }

    public static void SetPlayerHpStamina(int hp, int stamina)
    {
        EnsureInitialized();
        Current.player.hp = hp;
        Current.player.stamina = stamina;
    }

    public static void SetPlayerPosition(UnityEngine.Vector3 pos, float rotY)
    {
        EnsureInitialized();
        Current.player.pos = new Vector3DTO(pos);
        Current.player.rotY = rotY;
    }

    public static void AddSkillUnlocked(string skillId)
    {
        if (string.IsNullOrWhiteSpace(skillId)) return;
        EnsureInitialized();
        if (!Current.skillsUnlocked.Contains(skillId))
            Current.skillsUnlocked.Add(skillId);
    }

    public static void AddDeadEnemy(string enemyId)
    {
        if (string.IsNullOrWhiteSpace(enemyId)) return;
        EnsureInitialized();
        if (!Current.deadEnemies.Contains(enemyId))
            Current.deadEnemies.Add(enemyId);
    }

    public static void Reset()
    {
        Current = new SaveSlotDTO();
    }
}
