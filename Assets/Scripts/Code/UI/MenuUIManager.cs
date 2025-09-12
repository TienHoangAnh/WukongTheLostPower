using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;      // Canvas gốc của menu
    [SerializeField] private GameObject mainPanel;     // Panel menu chính
    [SerializeField] private GameObject settingsPanel; // Panel settings
    [SerializeField] private GameObject uiIngameForPC; // UI ingame

    [Header("Input & Pause")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private bool pauseAffectsTimeScale = true;
    [SerializeField] private bool manageCursor = true;
    [SerializeField] private float toggleCooldown = 0.2f;

    [Header("New Game")]
    [SerializeField] private string newGameSceneName = ""; // để trống = reload scene hiện tại

    [Header("Settings Presets")]
    [SerializeField]
    private ResolutionPreset[] presets = new ResolutionPreset[]
    {
        new ResolutionPreset("Fullscreen (native)", 0, 0),
        new ResolutionPreset("1500 x 800 (windowed)", 1500, 800),
    };
    [Range(0f, 1f)][SerializeField] private float defaultMasterVolume = 1f;

    [Header("UI Focus (tuỳ chọn)")]
    [SerializeField] private GameObject firstSelectedMain;
    [SerializeField] private GameObject firstSelectedSettings;

    // ==== State ====
    public enum UiState { Ingame, MainMenu, Settings }
    private UiState _state = UiState.Ingame;
    public bool IsMenuOpen { get; private set; }

    // ==== Pref keys ====
    const string PREF_RES_INDEX = "pref_res_index";
    const string PREF_FULLSCREEN = "pref_fullscreen";
    const string PREF_VOLUME = "pref_master_volume";

    float _prevTimeScale = 1f;
    float _nextToggleAllowedTime = 0f;

    void Awake()
    {
        // Trạng thái mặc định
        SafeSet(menuRoot, false);
        SafeSet(mainPanel, false);
        SafeSet(settingsPanel, false);
        SafeSet(uiIngameForPC, true); // UI ingame hiển thị ban đầu

        AudioListener.volume = PlayerPrefs.GetFloat(PREF_VOLUME, defaultMasterVolume);
        IsMenuOpen = false;
        _state = UiState.Ingame;
    }

    void Start()
    {
        ApplySavedSettings();
    }

    void Update()
    {
        if (Time.unscaledTime < _nextToggleAllowedTime) return;

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
            _nextToggleAllowedTime = Time.unscaledTime + toggleCooldown;
        }
    }

    // ===== Public API: nối vào Button/Toggle/Slider =====
    public void ToggleMenu()
    {
        // ESC: Settings -> Main, Main -> Ingame, Ingame -> Main
        if (_state == UiState.Settings) ApplyUiState(UiState.MainMenu);
        else ApplyUiState(_state == UiState.MainMenu ? UiState.Ingame : UiState.MainMenu);
    }

    public void ResumeGame() => ApplyUiState(UiState.Ingame);
    public void ContinueButton() => ApplyUiState(UiState.Ingame);
    public void OpenSettings() => ApplyUiState(UiState.Settings);
    public void BackFromSettings() => ApplyUiState(UiState.MainMenu);

    public void NewGame()
    {
        // (tuỳ bạn thêm popup xác nhận trước khi xoá save)
        // Đảm bảo rời trạng thái pause
        if (pauseAffectsTimeScale) Time.timeScale = 1f;
        IsMenuOpen = false;
#if UNITY_STANDALONE || UNITY_EDITOR
        if (manageCursor) { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }
#endif
        SafeSet(uiIngameForPC, true);
        SafeSet(menuRoot, false);
        SafeSet(mainPanel, false);
        SafeSet(settingsPanel, false);
        _state = UiState.Ingame;

        if (!string.IsNullOrEmpty(newGameSceneName))
            SceneManager.LoadScene(newGameSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // UI – Toggle fullscreen (Toggle onValueChanged)
    public void SetFullscreen(bool on)
    {
        PlayerPrefs.SetInt(PREF_FULLSCREEN, on ? 1 : 0);
        ApplyResolution(PlayerPrefs.GetInt(PREF_RES_INDEX, 0), on);
        PlayerPrefs.Save();
    }

    // UI – Chọn preset độ phân giải (Dropdown/TMP_Dropdown index)
    public void SetResolutionByIndex(int index)
    {
        index = Mathf.Clamp(index, 0, presets.Length - 1);
        PlayerPrefs.SetInt(PREF_RES_INDEX, index);
        bool on = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
        ApplyResolution(index, on);
        PlayerPrefs.Save();
    }

    // UI – Slider âm lượng 0..1
    public void SetMasterVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PREF_VOLUME, AudioListener.volume);
        PlayerPrefs.Save();
    }

    // ===== Core: áp state độc quyền 3 UI =====
    private void ApplyUiState(UiState next)
    {
        // Nếu chuyển sang trạng thái giống hiện tại thì vẫn cho chạy để đồng bộ UI
        _state = next;
        IsMenuOpen = (next != UiState.Ingame);

        // 1) Hiển thị độc quyền
        SafeSet(uiIngameForPC, next == UiState.Ingame);
        SafeSet(mainPanel, next == UiState.MainMenu);
        SafeSet(settingsPanel, next == UiState.Settings);
        SafeSet(menuRoot, next != UiState.Ingame);

        // 2) TimeScale
        if (pauseAffectsTimeScale)
        {
            if (next == UiState.Ingame)
            {
                Time.timeScale = (_prevTimeScale <= 0f) ? 1f : _prevTimeScale;
            }
            else
            {
                _prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }

        // 3) Cursor (desktop/editor)
#if UNITY_STANDALONE || UNITY_EDITOR
        if (manageCursor)
        {
            bool show = (next != UiState.Ingame);
            Cursor.visible = show;
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        }
#endif

        // 4) Focus cho gamepad/keyboard
        if (EventSystem.current)
        {
            GameObject first =
                (next == UiState.Settings) ? firstSelectedSettings :
                (next == UiState.MainMenu) ? firstSelectedMain :
                null;
            EventSystem.current.SetSelectedGameObject(first);
        }
    }

    // ===== Settings load/apply =====
    private void ApplySavedSettings()
    {
        int savedIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREF_RES_INDEX, 0), 0, presets.Length - 1);
        bool savedFullscreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
        ApplyResolution(savedIndex, savedFullscreen);
        AudioListener.volume = PlayerPrefs.GetFloat(PREF_VOLUME, defaultMasterVolume);
    }

    private void ApplyResolution(int index, bool fullscreen)
    {
        var p = presets[Mathf.Clamp(index, 0, presets.Length - 1)];

        // Trên desktop/editor mới áp resolution/fullscreen; mobile không cần
#if UNITY_STANDALONE || UNITY_EDITOR
        int w = p.width == 0 ? Display.main.systemWidth : p.width;
        int h = p.height == 0 ? Display.main.systemHeight : p.height;
        var mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.SetResolution(w, h, mode);
#endif
    }

    // ===== Utils =====
    private void SafeSet(GameObject go, bool active)
    {
        if (go && go.activeSelf != active) go.SetActive(active);
    }

    private void OnDisable()
    {
        if (IsMenuOpen && pauseAffectsTimeScale)
            Time.timeScale = (_prevTimeScale <= 0f) ? 1f : _prevTimeScale;
    }

    private void OnDestroy()
    {
        if (IsMenuOpen && pauseAffectsTimeScale)
            Time.timeScale = (_prevTimeScale <= 0f) ? 1f : _prevTimeScale;
    }

    [System.Serializable]
    public struct ResolutionPreset
    {
        public string label;
        public int width;
        public int height;
        public ResolutionPreset(string label, int w, int h)
        { this.label = label; this.width = w; this.height = h; }
    }
}
