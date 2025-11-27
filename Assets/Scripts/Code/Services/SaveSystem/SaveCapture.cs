using System;
using UnityEngine;
using System.Collections.Generic;

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

            // Save all collected item counts (id -> count)
            if (dto.collectedCounts == null)
                dto.collectedCounts = new Dictionary<string, int>();
            dto.collectedCounts.Clear();
            var all = inventory.GetType().GetMethod("GetAll")?.Invoke(inventory, null) as Dictionary<string, int>;
            if (all != null)
            {
                foreach (var kv in all)
                    dto.collectedCounts[kv.Key] = kv.Value;
            }
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
