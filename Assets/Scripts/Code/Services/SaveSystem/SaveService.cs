using System.Threading.Tasks;
using UnityEngine;

public static class SaveService
{
    public static async Task SaveGame(string slotName = "slotA")
    {
        var player = Object.FindFirstObjectByType<PlayerStats>();
        var chapter = Object.FindFirstObjectByType<ChapterManager>();
        var inventory = global::InventoryService.Instance;
        var skills = Object.FindFirstObjectByType<PlayerSkillManager>();

        var dto = SaveCapture.CaptureFromGameplay(slotName, "Player", player, chapter, inventory, skills);

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
            player.SetStats(dto.player.hp, dto.player.stamina);  // ✅ Giờ đã có hàm
            player.transform.position = dto.player.pos.ToVector3();
            player.transform.rotation = Quaternion.Euler(0, dto.player.rotY, 0);
        }
    }
}
