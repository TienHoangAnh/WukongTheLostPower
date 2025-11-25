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

    // Map index is 1-based to match your design (1..N)
    [Header("Maps (1-based)")]
    [SerializeField] private string[] mapSceneNames = { "", "Map1", "Map2", "Map3", "Map4", "Map5" };

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

            // DO NOT overwrite slotName with player name
            // Ensure playerName is set
            if (string.IsNullOrEmpty(dto.playerName))
                dto.playerName = playerName;

            // Start from map 1 for a new game
            dto.currentMap = 1;

            // Reset hp/stamina to full for starting the run, but keep unlocked skills
            if (dto.player == null) dto.player = new PlayerStateDTO();
            dto.player.hp = 100;
            dto.player.stamina = 100;

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

            if (string.IsNullOrEmpty(dto.playerName))
                dto.playerName = playerName;

            // Ensure player state exists
            if (dto.player == null) dto.player = new PlayerStateDTO();

            int targetMap = (dto.currentMap <= 0) ? 1 : dto.currentMap;

            // If the saved player HP is 0 (player had died and was returned to main menu),
            // resurrect the player: reset hp/stamina to full and clear inventory/collected counts
            if (dto.player.hp <= 0)
            {
                Debug.Log("[MainMenu] Detected save with player HP = 0. Resetting HP/Stamina to full and clearing inventory to avoid item abuse.");

                dto.player.hp = 100;
                dto.player.stamina = 100;

                // Clear inventory snapshot
                dto.inventory = new InventorySnapshot();

                // Clear per-item collected counts to prevent previously collected items from being kept
                dto.collectedCounts = new System.Collections.Generic.Dictionary<string, int>();

                // Reset map to first when resurrecting to avoid jumping due to stale ChapterManager/currentMap
                dto.currentMap = 1;

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
                targetMap = (dto.currentMap <= 0) ? 1 : dto.currentMap;
            }
            else
            {
                // Normal continue — restore saved hp/stamina and items
                SaveRuntime.Current = dto;
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
                string sceneName = SceneNameForMap(local.currentMap <= 0 ? 1 : local.currentMap);

                // Ensure chapter sync
                EnsureChapterIsSetOnNextLoad(local.currentMap <= 0 ? 1 : local.currentMap);

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

    // Helper: ensure ChapterManager.currentMap matches the map we're about to load.
    // If ChapterManager already exists, set immediately. Otherwise set on next scene load.
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

    // -------- HELPERS --------
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
        if (mapIndex >= 0 && mapIndex < mapSceneNames.Length)
        {
            string s = mapSceneNames[mapIndex];
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

    /// <summary>
    /// Gọi khi muốn lưu checkpoint theo scene hiện tại. 
    /// </summary>
    public void SaveGame(string sceneName)
    {
        PlayerPrefs.SetString(LastSaveKey, sceneName);
        PlayerPrefs.Save();

        // Tìm index của scene trong mapSceneNames để cập nhật currentMap
        int idx = 1;
        for (int i = 1; i < mapSceneNames.Length; i++)
            if (string.Equals(mapSceneNames[i], sceneName, StringComparison.OrdinalIgnoreCase))
            { idx = i; break; }

        if (SaveRuntime.Current == null) SaveRuntime.Current = new SaveSlotDTO();
        SaveRuntime.Current.currentMap = idx;
        _ = CloudSaveManager.SaveNow(SaveRuntime.Current);

        Debug.Log($"Game saved at map/scene: {sceneName} (index={idx})");
    }
}
