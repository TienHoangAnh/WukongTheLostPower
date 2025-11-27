using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;      // Gốc của menu ESC (thường = UIMainMenu)
    [SerializeField] private GameObject mainPanel;     // Panel menu chính (thường = UIMainMenu luôn)
    [SerializeField] private GameObject uiIngameForPC; // HUD ingame

    [Header("Input & Pause")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private bool pauseAffectsTimeScale = true;
    [SerializeField] private bool manageCursor = true;
    [SerializeField] private float toggleCooldown = 0.2f;

    [Header("New Game")]
    [SerializeField] private string newGameSceneName = ""; // để trống = reload scene hiện tại

    [Header("Save & Quit")]
    [SerializeField] private KeyCode saveAndQuitKey = KeyCode.F9;

    // ==== State ====
    public enum UiState { Ingame, MainMenu }
    private UiState _state = UiState.Ingame;
    public bool IsMenuOpen { get; private set; }

    float _prevTimeScale = 1f;
    float _nextToggleAllowedTime = 0f;

    void Awake()
    {
        // Singleton + giữ lại MenuUIManager xuyên scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Nếu designer quên gán trong Inspector thì auto tìm theo tên
        AutoResolvePanels();

        // Trạng thái mặc định khi vào ingame:
        //  - HUD bật
        //  - Menu ESC tắt
        SafeSet(menuRoot, false);
        SafeSet(mainPanel, false);
        SafeSet(uiIngameForPC, true);

        IsMenuOpen = false;
        _state = UiState.Ingame;
    }

    void Start()
    {
        // Tự bắt các nút Continue/NewGame/Exit/SaveAndQuit theo tên
        TryAutoBindMenuButtons();
    }

    void Update()
    {
        if (Time.unscaledTime < _nextToggleAllowedTime) return;

        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("[MenuUI] ESC pressed -> toggle menu");
            ToggleMenu();
            _nextToggleAllowedTime = Time.unscaledTime + toggleCooldown;
        }

        if (Input.GetKeyDown(saveAndQuitKey))
        {
            _ = SaveAndQuitAsync();
        }
    }

    // ===== Public API: nối vào Button/Toggle/Slider =====
    public void ToggleMenu()
    {
        // Toggle giữa Ingame <-> MainMenu (pause menu)
        ApplyUiState(_state == UiState.MainMenu ? UiState.Ingame : UiState.MainMenu);
    }

    public void ResumeGame() => ApplyUiState(UiState.Ingame);
    public void ContinueButton() => ApplyUiState(UiState.Ingame);

    public void NewGame()
    {
        // (tuỳ bạn thêm popup xác nhận trước khi xoá save)
        // Đảm bảo rời trạng thái pause
        if (pauseAffectsTimeScale) Time.timeScale = 1f;
        IsMenuOpen = false;

#if UNITY_STANDALONE || UNITY_EDITOR
        if (manageCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
#endif

        SafeSet(uiIngameForPC, false);
        SafeSet(menuRoot, false);
        SafeSet(mainPanel, false);
        _state = UiState.Ingame;

        if (!string.IsNullOrEmpty(newGameSceneName))
            SceneManager.LoadScene(newGameSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Persist SaveRuntime.Current to local and cloud, persist local item save, then return to MainMenu scene.
    /// </summary>
    public async Task SaveAndQuitAsync()
    {
        Debug.Log("[MenuUI] SaveAndQuit triggered.");

        // Ensure runtime exists
        if (SaveRuntime.Current == null) SaveRuntime.Current = new SaveSlotDTO();

        // Persist runtime to local and cloud
        try
        {
            await CloudSaveManager.SaveNow(SaveRuntime.Current);
            Debug.Log("[MenuUI] SaveRuntime saved (local + cloud attempted).");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MenuUI] CloudSaveManager.SaveNow failed: {ex.Message}");
        }

        // Persist local item save (GameSaveController / SaveSystem)
        try
        {
            if (GameSaveController.I != null && GameSaveController.I.Data != null)
            {
                SaveSystem.Save(GameSaveController.I.Data);
                Debug.Log("[MenuUI] Local item save persisted.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MenuUI] Failed to persist local item save: {ex.Message}");
        }

        // Finally load main menu scene
        if (LoadingScreen.I != null)
            LoadingScreen.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }

    // ===== Core: áp state độc quyền UI =====
    private void ApplyUiState(UiState next)
    {
        _state = next;
        IsMenuOpen = (next != UiState.Ingame);

        // 1) Hiển thị độc quyền
        SafeSet(uiIngameForPC, next == UiState.Ingame);
        SafeSet(mainPanel, next == UiState.MainMenu);
        SafeSet(menuRoot, next == UiState.MainMenu); // menuRoot = mainPanel trong case ingame

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
    }

    // ===== Utils =====
    private void SafeSet(GameObject go, bool active)
    {
        if (go && go.activeSelf != active)
            go.SetActive(active);
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

    /// <summary>
    /// Tự gán menuRoot, mainPanel, uiIngameForPC nếu quên kéo trong Inspector.
    /// </summary>
    private void AutoResolvePanels()
    {
        if (menuRoot == null)
        {
            var go = GameObject.Find("UIMainMenu");
            if (go != null)
            {
                menuRoot = go;
                Debug.Log("[MenuUI] Auto-assigned menuRoot = UIMainMenu");
            }
        }

        if (mainPanel == null)
        {
            mainPanel = menuRoot;
            if (mainPanel != null)
                Debug.Log("[MenuUI] Auto-assigned mainPanel = menuRoot");
        }

        if (uiIngameForPC == null)
        {
            var go = GameObject.Find("UIIngameForPC");
            if (go != null)
            {
                uiIngameForPC = go;
                Debug.Log("[MenuUI] Auto-assigned uiIngameForPC = UIIngameForPC");
            }
        }

        if (menuRoot == null || mainPanel == null || uiIngameForPC == null)
        {
            Debug.LogWarning("[MenuUI] Some panel references are still null. Please assign menuRoot, mainPanel, uiIngameForPC in Inspector.");
        }
    }

    // Tự bind các nút Continue/NewGame/Exit/SaveAndQuit theo tên
    private void TryAutoBindMenuButtons()
    {
        if (mainPanel == null) return;

        var buttons = mainPanel.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            var name = b.gameObject.name.ToLower();

            if (name.Contains("continue") || name.Contains("resume"))
            {
                b.onClick.AddListener(ContinueButton);
            }
            else if (name.Contains("new") || name.Contains("newgame"))
            {
                b.onClick.AddListener(NewGame);
            }
            else if (name.Contains("exit") || name.Contains("quit"))
            {
                b.onClick.AddListener(() => Application.Quit());
            }
            else if (name.Contains("save") || name.Contains("saveandquit"))
            {
                b.onClick.AddListener(() => _ = SaveAndQuitAsync());
            }
        }
    }
}
