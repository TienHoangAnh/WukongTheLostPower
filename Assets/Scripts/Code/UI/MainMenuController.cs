using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    private const string LastSaveKey = "LastSaveScene";

    [Header("UI References")]
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private GameObject namePanel;
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private TMP_Text warningText;

    // Map index is1-based to match your design (1..N)
    [Header("Maps (1-based)")]
    [SerializeField] private string[] mapSceneNames = {"Map1", "Map2", "Map3", "Map4", "Map5" };

    private bool _isContinue = false;

    private void Start()
    {
        if (namePanel) namePanel.SetActive(false);
        if (buttonsPanel) buttonsPanel.SetActive(true);
        if (warningText) warningText.gameObject.SetActive(false);
    }

    public void OnContinueClicked()
    {
        _isContinue = true;
        ShowNamePanel();
    }

    public void OnNewGameClicked()
    {
        _isContinue = false;
        ShowNamePanel();
    }

    public void OnLeaderboard()
    {
        if (LoadingScreen.I != null) LoadingScreen.LoadScene("Leaderboard"); else SceneManager.LoadScene("Leaderboard");
    }

    public void OnSettings()
    {
        var settingsPanelGo = GameObject.Find("SettingsPanel");
        if (settingsPanelGo) settingsPanelGo.SetActive(true);
        else Debug.Log("Settings panel not found!");
    }

    public void OnExit()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    public void OnConfirmName()
    {
        if (!TryGetPlayerName(out var playerName)) return;

        if (_isContinue) _ = ContinueAsync(playerName);
        else _ = NewGameAsync(playerName);
    }

    public void OnBackFromName()
    {
        if (namePanel) namePanel.SetActive(false);
        if (buttonsPanel) buttonsPanel.SetActive(true);
        if (warningText) warningText.gameObject.SetActive(false);
    }

    // -------- NEW GAME --------
    private async Task NewGameAsync(string playerName)
    {
        try
        {
            // Local-first load or create
            var dto = await CloudSaveManager.TryLoadOrCreate("slotA", playerName);

            dto.playerName = playerName;

            // Start from map1 for a new game
            dto.currentMap = 1;

            // Reset hp/stamina to full for starting the run, but keep unlocked skills
            if (dto.player == null) dto.player = new PlayerStateDTO();
            dto.player.hp =100;
            dto.player.stamina =100;

            // Clear unlocked skills and cooldowns for a new game so the player starts fresh
            dto.skillsUnlocked = new System.Collections.Generic.List<string>();
            dto.skillCooldowns = new System.Collections.Generic.Dictionary<string, float>();

            // Reset inventory / collected counts / progression fields
            dto.inventory = new InventorySnapshot();
            dto.collectedCounts = new System.Collections.Generic.Dictionary<string, int>();
            dto.essencesCollected =0;
            dto.playTimeSeconds =0f;
            dto.deathCount =0;
            dto.bossesDefeated = new System.Collections.Generic.List<string>();
            dto.deadEnemies = new System.Collections.Generic.List<string>();
            dto.worldFlags = new System.Collections.Generic.Dictionary<string, bool>();

            // Apply to runtime and persist
            SaveRuntime.Current = dto;

            // Ensure Firebase/runtime is ready and persist the new slot immediately
#if FIREBASE_ENABLED
 try
 {
 await FirebaseRuntime.EnsureInitializedAsync();
 }
 catch (Exception e)
 {
 Debug.LogWarning($"[MainMenu] Firebase init failed when saving new game: {e.Message}");
 }
#endif
 Debug.Log("[MainMenu] Saving new slot to local/cloud before loading Map1...");
 await CloudSaveManager.SaveNow(dto);
 
 // Reset local GameSaveController (collected items) if present so runtime/local save mirrors cleared DTO
 try
 {
 var g = GameSaveController.I;
 if (g != null)
 {
 g.Data.collectedCounts = new System.Collections.Generic.Dictionary<string, int>();
 g.Data.collectedIds = new System.Collections.Generic.List<string>();
 g.CollectedIds.Clear();
 SaveSystem.Save(g.Data);
#if UNITY_EDITOR
 Debug.Log("[MainMenu] Cleared local GameSaveController data for New Game.");
#endif
 }

 // Clear runtime InventoryManager contents if present
 var inv = InventoryManager.I;
 if (inv != null)
 {
 var all = inv.GetAll();
 var keys = new System.Collections.Generic.List<string>(all.Keys);
 foreach (var k in keys)
 {
 int count = inv.GetCount(k);
 if (count >0)
 inv.UseItem(k, count);
 }
 }
 }
 catch (System.Exception ex)
 {
 Debug.LogWarning($"[MainMenu] Failed to clear local inventory runtime: {ex.Message}");
 }

 // If a PlayerSkillManager exists (for example attached to a persistent Player), instruct it to reload from the cleared save
 var skillMgr = FindFirstObjectByType<PlayerSkillManager>();
 if (skillMgr != null)
 {
 try { skillMgr.ReloadFromSaveRuntime(); }
 catch { /* non-fatal: older versions may not have the method */ }
 }

 // Ensure persistent PlayerStats (if present) reflect the freshly reset HP/Stamina
 try
 {
 var ps = PlayerStats.Instance;
 if (ps != null)
 {
 ps.SetStats(dto.player.hp, dto.player.stamina);
 }
 }
 catch (System.Exception ex)
 {
 Debug.LogWarning($"[MainMenu] Failed to reset PlayerStats for New Game: {ex.Message}");
 }

 string scene = SceneNameForMap(1);
 PlayerPrefs.SetString(LastSaveKey, scene);
 PlayerPrefs.Save();

 // Ensure ChapterManager reflects the chosen starting map
 EnsureChapterIsSetOnNextLoad(1);

 if (LoadingScreen.I != null) LoadingScreen.LoadScene(scene); else SceneManager.LoadScene(scene);
 }
 catch (Exception ex)
 {
 Debug.LogError($"[MainMenu] NewGame failed: {ex}");
 FallbackToMap1();
 }
 }

    // -------- CONTINUE --------
    private async Task ContinueAsync(string playerName)
    {
        try
        {
            var dto = await CloudSaveManager.TryLoadOrCreate("slotA", playerName);

            // If remote/local slot exists but playerName doesn't match entered name, treat as not found
            if (!string.Equals(dto.playerName, playerName, StringComparison.Ordinal))
            {
                Debug.LogWarning($"[MainMenu] Continue: no save found for player name '{playerName}'. Found slot playerName='{dto.playerName ?? "(null)"}'");
                if (warningText)
                {
                    warningText.text = $"No save found for player '{playerName}'. Please check name or New Game.";
                    warningText.gameObject.SetActive(true);
                }
                // keep name panel open for correction
                if (namePanel) namePanel.SetActive(true);
                if (buttonsPanel) buttonsPanel.SetActive(false);
                return;
            }

            if (string.IsNullOrEmpty(dto.playerName))
                dto.playerName = playerName;

            // Ensure player state exists
            if (dto.player == null) dto.player = new PlayerStateDTO();

            int targetMap = (dto.currentMap <=0) ?1 : dto.currentMap;

            // If the saved player HP is0 (player had died and was returned to main menu),
            // resurrect the player: reset hp/stamina to full and clear inventory/collected counts
            if (dto.player.hp <=0)
            {
                Debug.Log("[MainMenu] Detected save with player HP =0. Resetting HP/Stamina to full and clearing inventory to avoid item abuse.");

                dto.player.hp =100;
                dto.player.stamina =100;

                // Clear inventory snapshot
                dto.inventory = new InventorySnapshot();

                // Clear per-item collected counts to prevent previously collected items from being kept
                dto.collectedCounts = new System.Collections.Generic.Dictionary<string, int>();

                // Reset map to first when resurrecting to avoid jumping due to stale ChapterManager/currentMap
                dto.currentMap =1;

                // Persist the updated dto so continue starts from a clean resurrected state
                SaveRuntime.Current = dto;
                await CloudSaveManager.SaveNow(dto);

                // Also clear local item save (GameSaveController / SaveSystem) so local 'collected' flags won't allow skipping
                try
                {
                    var g = GameSaveController.I;
                    if (g != null)
                    {
                        g.Data.collectedCounts = new System.Collections.Generic.Dictionary<string, int>();
                        g.Data.collectedIds = new System.Collections.Generic.List<string>();
                        g.CollectedIds.Clear();
                        SaveSystem.Save(g.Data);
#if UNITY_EDITOR
                        Debug.Log("[MainMenu] Cleared local item save (GameSaveController).");
#endif
                    }
                    else
                    {
                        // If GameSaveController not present, write an empty save file anyway
                        SaveSystem.Save(new SaveData { collectedCounts = new System.Collections.Generic.Dictionary<string, int>(), collectedIds = new System.Collections.Generic.List<string>() });
#if UNITY_EDITOR
                        Debug.Log("[MainMenu] Cleared local item save (SaveSystem fallback).");
#endif
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[MainMenu] Failed to clear local item save: {ex.Message}");
                }

                // recompute targetMap in case dto.currentMap changed above
                targetMap = (dto.currentMap <=0) ?1 : dto.currentMap;
            }
            else
            {
                // Normal continue — restore saved hp/stamina and items
                SaveRuntime.Current = dto;

                // Ensure runtime systems pick up the loaded DTO once the target scene has been loaded.
                void OnApply(Scene s, LoadSceneMode m)
                {
                    try
                    {
                        SyncRuntimeFromDto(dto);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[MainMenu] Failed to apply save DTO to runtime after scene load: {ex.Message}");
                    }
                    SceneManager.sceneLoaded -= OnApply;
                }

                SceneManager.sceneLoaded += OnApply;
            }

            string sceneName = SceneNameForMap(targetMap);

            PlayerPrefs.SetString(LastSaveKey, sceneName);
            PlayerPrefs.Save();

            // Ensure ChapterManager reflects the loaded map (important because ChapterManager is DontDestroyOnLoad)
            EnsureChapterIsSetOnNextLoad(targetMap);

            if (LoadingScreen.I != null) LoadingScreen.LoadScene(sceneName); else SceneManager.LoadScene(sceneName);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MainMenu] Continue error: {ex.Message}. Trying local/fallback…");

            if (CloudSaveManager.TryLoadLocal(out var local))
            {
                SaveRuntime.Current = local;
                string sceneName = SceneNameForMap(local.currentMap <=0 ?1 : local.currentMap);

                // Ensure chapter sync
                EnsureChapterIsSetOnNextLoad(local.currentMap <=0 ?1 : local.currentMap);

                if (LoadingScreen.I != null) LoadingScreen.LoadScene(sceneName); else SceneManager.LoadScene(sceneName);
                return;
            }

            if (PlayerPrefs.HasKey(LastSaveKey))
            {
                string lastScene = PlayerPrefs.GetString(LastSaveKey);
                if (LoadingScreen.I != null) LoadingScreen.LoadScene(lastScene); else SceneManager.LoadScene(lastScene);
            }
            else
            {
                FallbackToMap1();
            }
        }
    }

    private void EnsureChapterIsSetOnNextLoad(int mapIndex)
    {
        if (ChapterManager.Instance != null)
        {
            ChapterManager.Instance.currentMap = mapIndex;
            Debug.Log($"[MainMenu] ChapterManager currentMap set to {mapIndex} immediately.");
            return;
        }

        // Otherwise, set once after the next scene loads
        void OnLoaded(Scene s, LoadSceneMode m)
        {
            if (ChapterManager.Instance != null)
            {
                ChapterManager.Instance.currentMap = mapIndex;
                Debug.Log($"[MainMenu] ChapterManager currentMap set to {mapIndex} on sceneLoaded.");
            }
            SceneManager.sceneLoaded -= OnLoaded;
        }

        SceneManager.sceneLoaded += OnLoaded;
    }

    // Sync DTO into runtime systems after scene load
    private void SyncRuntimeFromDto(SaveSlotDTO dto)
    {
        if (dto == null) return;

        //1) Sync GameSaveController / local item save
        try
        {
            if (GameSaveController.I != null)
            {
                var g = GameSaveController.I;

                // Reset in-memory then mark collected from DTO
                g.ResetCollectedInMemory(false);

                if (dto.collectedCounts != null)
                {
                    foreach (var kv in dto.collectedCounts)
                    {
                        if (kv.Value <=0) continue;
                        // MarkCollected will update Data and persist via SaveSystem.Save
                        g.MarkCollected(kv.Key, kv.Value);
                    }
                }
            }
            else
            {
                // Persist a local item save file so other systems can pick it up later
                var d = new SaveData();
                d.collectedCounts = dto.collectedCounts != null ? new System.Collections.Generic.Dictionary<string, int>(dto.collectedCounts) : new System.Collections.Generic.Dictionary<string, int>();
                d.collectedIds = new System.Collections.Generic.List<string>();
                if (dto.collectedCounts != null)
                {
                    foreach (var kv in dto.collectedCounts)
                    if (kv.Value >0 && !d.collectedIds.Contains(kv.Key)) d.collectedIds.Add(kv.Key);
                }
                SaveSystem.Save(d);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenu] Failed to sync GameSaveController from DTO: {ex.Message}");
        }

        //2) Sync InventoryManager runtime counts
        try
        {
            var inv = InventoryManager.I;
            if (inv != null && dto.collectedCounts != null)
            {
                foreach (var kv in dto.collectedCounts)
                {
                    int desired = kv.Value;
                    int current = inv.GetCount(kv.Key);
                    if (desired > current) inv.AddItem(kv.Key, desired - current);
                    else if (desired < current) inv.UseItem(kv.Key, current - desired);
                }

                // Also ensure common quick-items match snapshot
                if (dto.inventory != null)
                {
                    void Ensure(string id, int val)
                    {
                        int cur = inv.GetCount(id);
                        if (val > cur) inv.AddItem(id, val - cur);
                        else if (val < cur) inv.UseItem(id, cur - val);
                    }

                    Ensure("holy_water", dto.inventory.holy_water);
                    Ensure("elixir", dto.inventory.elixir);
                    Ensure("natural_energy", dto.inventory.natural_energy);
                    Ensure("power_pill", dto.inventory.power_pill);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenu] Failed to sync InventoryManager from DTO: {ex.Message}");
        }

        //3) Restore player stats/position immediately if a persistent PlayerStats exists
        try
        {
            var ps = PlayerStats.Instance;
            if (ps != null && dto.player != null)
            {
                ps.SetStats(dto.player.hp, dto.player.stamina);
                if (dto.player.pos != null)
                {
                    var v = dto.player.pos.ToVector3();
                    ps.transform.position = v;
                    ps.transform.rotation = Quaternion.Euler(0f, dto.player.rotY,0f);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenu] Failed to restore PlayerStats from DTO: {ex.Message}");
        }

        //4) Reload PlayerSkillManager bindings/cooldowns
        try
        {
            var skillMgr = FindFirstObjectByType<PlayerSkillManager>();
            if (skillMgr != null) skillMgr.ReloadFromSaveRuntime();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenu] Failed to reload PlayerSkillManager from DTO: {ex.Message}");
        }
    }

    private void ShowNamePanel()
    {
        if (buttonsPanel) buttonsPanel.SetActive(false);
        if (namePanel) namePanel.SetActive(true);
        if (warningText) { warningText.gameObject.SetActive(false); warningText.text = ""; }
        if (playerNameInput)
        {
            playerNameInput.text = string.Empty;
            try { playerNameInput.ActivateInputField(); } catch { }
        }
    }

    private bool TryGetPlayerName(out string playerName)
    {
        playerName = null;
        if (playerNameInput == null)
        {
            playerName = "Player";
            return true;
        }

        var txt = playerNameInput.text?.Trim();
        if (string.IsNullOrWhiteSpace(txt))
        {
            if (warningText)
            {
                warningText.text = "Please enter a player name.";
                warningText.gameObject.SetActive(true);
            }
            try { playerNameInput.ActivateInputField(); } catch { }
            return false;
        }

        playerName = txt;
        return true;
    }

    private string SceneNameForMap(int mapIndex)
    {
        // mapIndex is treated as1-based. Convert to zero-based array index safely.
        if (mapSceneNames == null || mapSceneNames.Length ==0) return "Map1";

        int zeroBased = mapIndex -1;
        if (zeroBased >=0 && zeroBased < mapSceneNames.Length)
        {
            var s = mapSceneNames[zeroBased];
            if (!string.IsNullOrEmpty(s)) return s;
        }
        return "Map1";
    }

    private void FallbackToMap1()
    {
        PlayerPrefs.SetString(LastSaveKey, "Map1");
        PlayerPrefs.Save();
        if (LoadingScreen.I != null) LoadingScreen.LoadScene("Map1"); else SceneManager.LoadScene("Map1");
    }

    public void SaveGame(string sceneName)
    {
        PlayerPrefs.SetString(LastSaveKey, sceneName);
        PlayerPrefs.Save();

        // Tìm index của scene trong mapSceneNames để cập nhật currentMap (1-based)
        int idx =1; // default to map1
        for (int i =0; i < mapSceneNames.Length; i++)
        if (string.Equals(mapSceneNames[i], sceneName, StringComparison.OrdinalIgnoreCase))
 { idx = i +1; break; }

        if (SaveRuntime.Current == null) SaveRuntime.Current = new SaveSlotDTO();
        SaveRuntime.Current.currentMap = idx;
        _ = CloudSaveManager.SaveNow(SaveRuntime.Current);

        Debug.Log($"Game saved at map/scene: {sceneName} (index={idx})");
    }
}
