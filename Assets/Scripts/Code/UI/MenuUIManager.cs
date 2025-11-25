using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;      // Canvas gốc của menu
    [SerializeField] private GameObject mainPanel;     // Panel menu chính
    [SerializeField] private GameObject uiIngameForPC; // UI ingame

    [Header("Input & Pause")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private bool pauseAffectsTimeScale = true;
    [SerializeField] private bool manageCursor = true;
    [SerializeField] private float toggleCooldown = 0.2f;

    [Header("New Game")]
    [SerializeField] private string newGameSceneName = ""; // để trống = reload scene hiện tại

    // ==== State ====
    public enum UiState { Ingame, MainMenu }
    private UiState _state = UiState.Ingame;
    public bool IsMenuOpen { get; private set; }

    float _prevTimeScale = 1f;
    float _nextToggleAllowedTime = 0f;

    void Awake()
    {
        // Ensure singleton and persist across scenes so ingame UI isn't lost
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // If menuRoot or uiIngameForPC are assigned and belong to this scene, make them persistent too
            if (menuRoot != null) DontDestroyOnLoad(menuRoot);
            if (uiIngameForPC != null) DontDestroyOnLoad(uiIngameForPC);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Trạng thái mặc định
        SafeSet(menuRoot, false);
        SafeSet(mainPanel, false);
        SafeSet(uiIngameForPC, true); // UI ingame hiển thị ban đầu

        IsMenuOpen = false;
        _state = UiState.Ingame;
    }

    void Start()
    {
        // try to wire up buttons automatically so designer doesn't have to hook them in inspector
        TryAutoBindMenuButtons();
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
        // Toggle between MainMenu and Ingame
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
        if (manageCursor) { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }
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

    // ===== Core: áp state độc quyền3 UI =====
    private void ApplyUiState(UiState next)
    {
        // Nếu chuyển sang trạng thái giống hiện tại thì vẫn cho chạy để đồng bộ UI
        _state = next;
        IsMenuOpen = (next != UiState.Ingame);

        //1) Hiển thị độc quyền
        SafeSet(uiIngameForPC, next == UiState.Ingame);
        SafeSet(mainPanel, next == UiState.MainMenu);
        SafeSet(menuRoot, next != UiState.Ingame);

        //2) TimeScale
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

        //3) Cursor (desktop/editor)
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

    // Try to auto-bind common menu buttons by name so UI works without manual wiring in Inspector
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
        }

    }
}
