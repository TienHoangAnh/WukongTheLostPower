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
        SceneManager.LoadScene("Leaderboard");
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

            SaveRuntime.Current = dto;
            await CloudSaveManager.SaveNow(dto);

            string scene = SceneNameForMap(1);
            PlayerPrefs.SetString(LastSaveKey, scene);
            PlayerPrefs.Save();

            SceneManager.LoadScene(scene);
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

            SaveRuntime.Current = dto;

            int targetMap = (dto.currentMap <= 0) ? 1 : dto.currentMap;
            string sceneName = SceneNameForMap(targetMap);

            PlayerPrefs.SetString(LastSaveKey, sceneName);
            PlayerPrefs.Save();

            SceneManager.LoadScene(sceneName);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[MainMenu] Continue error: {ex.Message}. Trying local/fallback…");

            if (CloudSaveManager.TryLoadLocal(out var local))
            {
                SaveRuntime.Current = local;
                string sceneName = SceneNameForMap(local.currentMap <= 0 ? 1 : local.currentMap);
                SceneManager.LoadScene(sceneName);
                return;
            }

            if (PlayerPrefs.HasKey(LastSaveKey))
            {
                string lastScene = PlayerPrefs.GetString(LastSaveKey);
                SceneManager.LoadScene(lastScene);
            }
            else
            {
                FallbackToMap1();
            }
        }
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
        SceneManager.LoadScene("Map1");
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
