using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class WinScreenManager : MonoBehaviour
{
 public static WinScreenManager Instance { get; private set; }

 [Header("Win Screen Elements")]
 public GameObject winRoot; // assign root object for win UI
 public TextMeshProUGUI titleText;
 public TextMeshProUGUI descriptionText;
 public Button mainMenuButton;
 public Button exitButton;

 private void Awake()
 {
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

 if (winRoot != null)
 winRoot.SetActive(false);

 // safe button bindings
 if (mainMenuButton != null)
 {
 mainMenuButton.onClick.RemoveAllListeners();
 mainMenuButton.onClick.AddListener(() => _ = OnMainMenuClicked());
 }
 if (exitButton != null)
 {
 exitButton.onClick.RemoveAllListeners();
 exitButton.onClick.AddListener(OnExitClicked);
 }
 }

 public static void ShowVictory(string title = "You Won!", string description = "You have restored the power of Wukong.")
 {
 if (Instance == null)
 {
 // try to find existing in scene
 var go = GameObject.FindFirstObjectByType<WinScreenManager>();
 if (go != null) Instance = go;
 }

 if (Instance == null)
 {
 Debug.LogWarning("[WinScreenManager] No WinScreenManager instance found in scene to show victory UI.");
 // fallback: show a simple debug log and load MainMenu
 Debug.Log("Victory: " + title + " - " + description);
 return;
 }

 Instance.Show(title, description);
 }

 private void Show(string title, string description)
 {
 if (winRoot != null)
 winRoot.SetActive(true);

 if (titleText != null)
 titleText.text = title;
 if (descriptionText != null)
 descriptionText.text = description;

 // Optionally pause game
 Time.timeScale =0f;
 }

 private async Task OnMainMenuClicked()
 {
 // restore time scale
 Time.timeScale =1f;

 // If MenuUIManager exists, use its SaveAndQuit to persist; otherwise just load MainMenu
 if (MenuUIManager.Instance != null)
 {
 await MenuUIManager.Instance.SaveAndQuitAsync();
 }
 else if (LoadingScreen.I != null)
 {
 LoadingScreen.LoadScene("MainMenu");
 }
 else
 {
 SceneManager.LoadScene("MainMenu");
 }
 }

 private void OnExitClicked()
 {
 // restore time scale and quit
 Time.timeScale =1f;
 Application.Quit();
 }
}
