using System.Threading.Tasks;
using UnityEngine;

public static class SaveService
{
    public static async Task SaveGame(string slotName = "slotA")
    {
        // Ensure CloudSaveManager knows the active slot id
        CloudSaveManager.CurrentSlotId = slotName;

        var player = Object.FindFirstObjectByType<PlayerStats>();
        var chapter = Object.FindFirstObjectByType<ChapterManager>();
        var inventory = global::InventoryService.Instance;
        var skills = Object.FindFirstObjectByType<PlayerSkillManager>();

        var dto = SaveCapture.CaptureFromGameplay(slotName, "Player", player, chapter, inventory, skills);

        // Always write local copy first to ensure a local file exists even if remote fails
        try
        {
            LocalCache.Write(slotName, dto);
            Debug.Log($"[SaveService] Local save written: {Application.persistentDataPath}/save_{slotName}.json");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SaveService] Local save failed: {ex}");
        }

        // Then perform orchestrated save (will write local again and attempt remote)
        await CloudSaveManager.SaveNow(dto);
        Debug.Log($"💾 Game saved! HP:{dto.player.hp}, Map:{dto.currentMap}, Skills:{dto.skillsUnlocked.Count}");
    }

    public static async Task LoadGame(string slotName = "slotA")
    {
        var dto = await CloudSaveManager.TryLoadOrCreate(slotName, "Player");
        SaveRuntime.Apply(dto);

        Debug.Log($"📂 Game loaded: {dto.playerName} - HP:{dto.player.hp}, Map:{dto.currentMap}");

        var player = Object.FindFirstObjectByType<PlayerStats>();
        if (player != null)
        {
            player.SetStats(dto.player.hp, dto.player.stamina);
            player.transform.position = dto.player.pos.ToVector3();
            player.transform.rotation = Quaternion.Euler(0, dto.player.rotY, 0);
        }
    }
}
