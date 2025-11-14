using System;
using UnityEngine;

public static class SaveCapture
{
    public static SaveSlotDTO CaptureFromGameplay(string slotName,
                                                  string playerName,
                                                  PlayerStats playerStats,
                                                  ChapterManager chapter,
                                                  InventoryService inventory,
                                                  PlayerSkillManager skillManager)
    {
        var dto = SaveRuntime.Current ?? new SaveSlotDTO();

        // Thông tin cơ bản
        dto.slotName = slotName;
        dto.playerName = playerName;
        dto.currentMap = chapter?.currentMap ?? 1;
        dto.playTimeSeconds += Time.deltaTime;

        // Player State
        if (playerStats != null)
        {
            dto.player = new PlayerStateDTO
            {
                hp = Mathf.RoundToInt(playerStats.currentHealth),
                stamina = Mathf.RoundToInt(playerStats.currentStamina),
                flask = inventory?.GetCount("flask") ?? 0,
                rotY = playerStats.transform.eulerAngles.y,
                pos = new Vector3DTO(playerStats.transform.position)
            };
        }

        // Inventory
        if (inventory != null)
        {
            dto.inventory = new InventorySnapshot
            {
                holy_water = inventory.GetCount("holy_water"),
                elixir = inventory.GetCount("elixir"),
                power_pill = inventory.GetCount("power_pill")
            };
        }

        // Skill
        if (skillManager != null)
            dto.skillsUnlocked = skillManager.GetUnlockedIds();

        // Timestamp
        dto.lastSavedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        SaveRuntime.Current = dto;
        return dto;
    }
}
