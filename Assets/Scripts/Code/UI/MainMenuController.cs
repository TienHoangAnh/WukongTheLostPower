using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    private const string LastSaveKey = "LastSaveChapter";

    [Header("UI References")]
    [SerializeField] private TMP_InputField playerNameInput;  // TMP only
    [SerializeField] private GameObject namePanel;            // Panel: Input + Confirm + Back
    [SerializeField] private GameObject buttonsPanel;         // Panel: Continue/NewGame/Leaderboard/Settings/Exit
    [SerializeField] private TMP_Text warningText;            // (optional) chỗ hiện cảnh báo

    [Header("Chapters Map (1-based)")]
    [SerializeField] private string[] chapterSceneNames = { "", "Chapter1", "Chapter2" };

    private bool _isContinue = false; // true: Continue, false: NewGame

    private void Start()
    {
        if (namePanel) namePanel.SetActive(false);
        if (buttonsPanel) buttonsPanel.SetActive(true);
        if (warningText) warningText.gameObject.SetActive(false);
    }

    // ===== Top-level buttons =====
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

    // ===== Name panel buttons =====
    public void OnConfirmName()
    {
        if (!TryGetPlayerName(out var playerName)) return;

        if (_isContinue) _ = ContinueAsync(playerName);
        else _ = NewGameAsync(playerName);
    }

    public void OnBackFromName()
    {
        // Quay lại menu chính
        if (namePanel) namePanel.SetActive(false);
        if (buttonsPanel) buttonsPanel.SetActive(true);
        if (warningText) warningText.gameObject.SetActive(false);
    }

    // ===== Core flows =====
    private async Task NewGameAsync(string playerName)
    {
        try
        {
            var dto = await CloudSaveManager.TryLoadOrCreate("slotA", playerName);
            // Ép về chapter 1 cho New Game
            dto.slotName = playerName;
            if (string.IsNullOrEmpty(dto.playerName)) dto.playerName = playerName;  // cần field này trong SaveSlotDTO
            dto.chapterIndex = 1; // reset vị trí bắt đầu; nếu muốn reset thêm inventory/flags có thể thêm ở đây

            SaveRuntime.Current = dto;
            await CloudSaveManager.SaveNow(dto);

            string scene = SceneNameForChapter(1);
            PlayerPrefs.SetString(LastSaveKey, scene);
            PlayerPrefs.Save();

            SceneManager.LoadScene(scene);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MainMenu] NewGame failed: {ex}");
            FallbackToChapter1();
        }
    }

    private async Task ContinueAsync(string playerName)
    {
        try
        {
            var dto = await CloudSaveManager.TryLoadOrCreate("slotA", playerName);
            dto.slotName = playerName;
            if (string.IsNullOrEmpty(dto.playerName)) dto.playerName = playerName;

            SaveRuntime.Current = dto;

            int targetChapter = (dto.chapterIndex <= 0) ? 1 : dto.chapterIndex;
            string sceneName = SceneNameForChapter(targetChapter);

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
                string sceneName = SceneNameForChapter(local.chapterIndex <= 0 ? 1 : local.chapterIndex);
                SceneManager.LoadScene(sceneName);
                return;
            }

            if (PlayerPrefs.HasKey(LastSaveKey))
            {
                string lastChapter = PlayerPrefs.GetString(LastSaveKey);
                SceneManager.LoadScene(lastChapter);
            }
            else
            {
                FallbackToChapter1();
            }
        }
    }

    // ===== Helpers =====
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

    private string SceneNameForChapter(int chapterIndex)
    {
        if (chapterIndex >= 0 && chapterIndex < chapterSceneNames.Length)
        {
            string s = chapterSceneNames[chapterIndex];
            if (!string.IsNullOrEmpty(s)) return s;
        }
        return "Chapter1";
    }

    private void FallbackToChapter1()
    {
        PlayerPrefs.SetString(LastSaveKey, "Chapter1");
        PlayerPrefs.Save();
        SceneManager.LoadScene("Chapter1");
    }

    // (tuỳ) Cho phép gọi từ chỗ khác nếu bạn vẫn muốn giữ API này
    public void SaveGame(string chapterName)
    {
        PlayerPrefs.SetString(LastSaveKey, chapterName);
        PlayerPrefs.Save();

        int idx = 1;
        for (int i = 1; i < chapterSceneNames.Length; i++)
            if (string.Equals(chapterSceneNames[i], chapterName, StringComparison.OrdinalIgnoreCase))
            { idx = i; break; }

        if (SaveRuntime.Current == null) SaveRuntime.Current = new SaveSlotDTO();
        SaveRuntime.Current.chapterIndex = idx;
        _ = CloudSaveManager.SaveNow(SaveRuntime.Current);

        Debug.Log($"Game saved at chapter: {chapterName}");
    }
}
