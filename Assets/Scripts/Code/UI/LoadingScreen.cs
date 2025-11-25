using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Simple persistent loading screen. Attach to a GameObject with a CanvasGroup
/// and optional Slider/TMP_Text to show progress. Call `LoadingScreen.LoadScene(name)`
/// instead of `SceneManager.LoadScene(...)` to show the loading UI.
/// </summary>
public class LoadingScreen : MonoBehaviour
{
 public static LoadingScreen I { get; private set; }

 [Header("UI (optional)")]
 [Tooltip("CanvasGroup used to fade-in/out the loading overlay")]
 public CanvasGroup canvasGroup;
 [Tooltip("Progress bar (0..1) - optional")]
 public Slider progressBar;
 [Tooltip("Progress text - optional")]
 public TMP_Text progressText;

 [Header("Settings")]
 public float fadeDuration =3f;

 void Awake()
 {
 if (I == null)
 {
 I = this;
 DontDestroyOnLoad(gameObject);
 if (canvasGroup != null) canvasGroup.alpha =0f;
 }
 else if (I != this)
 {
 Destroy(gameObject);
 }
 }

 public static void LoadScene(string sceneName)
 {
 if (I == null)
 {
 // fallback
 SceneManager.LoadScene(sceneName);
 return;
 }
 I.StartCoroutine(I.LoadSceneRoutine(sceneName));
 }

 private IEnumerator LoadSceneRoutine(string sceneName)
 {
 yield return StartCoroutine(FadeIn());

 var op = SceneManager.LoadSceneAsync(sceneName);
 if (op == null)
 {
 Debug.LogError("LoadingScreen: failed to start async load.");
 yield return StartCoroutine(FadeOut());
 yield break;
 }

 op.allowSceneActivation = false;

 while (op.progress <0.9f)
 {
 UpdateProgress(op.progress /0.9f);
 yield return null;
 }

 // almost done
 UpdateProgress(1f);

 // give a small delay so player sees the completed bar
 yield return new WaitForSeconds(0.25f);

 op.allowSceneActivation = true;

 // wait until the scene is actually activated
 while (!op.isDone)
 yield return null;

 // optionally keep the loading screen for a short moment
 yield return new WaitForSeconds(0.1f);

 yield return StartCoroutine(FadeOut());
 }

 private void UpdateProgress(float t)
 {
 t = Mathf.Clamp01(t);
 if (progressBar != null) progressBar.value = t;
 if (progressText != null) progressText.text = Mathf.RoundToInt(t *100f) + "%";
 }

 private IEnumerator FadeIn()
 {
 if (canvasGroup == null)
 yield break;

 float elapsed =0f;
 while (elapsed < fadeDuration)
 {
 elapsed += Time.unscaledDeltaTime;
 canvasGroup.alpha = Mathf.Lerp(0f,1f, elapsed / fadeDuration);
 yield return null;
 }
 canvasGroup.alpha =1f;
 }

 private IEnumerator FadeOut()
 {
 if (canvasGroup == null)
 yield break;

 float elapsed =0f;
 while (elapsed < fadeDuration)
 {
 elapsed += Time.unscaledDeltaTime;
 canvasGroup.alpha = Mathf.Lerp(1f,0f, elapsed / fadeDuration);
 yield return null;
 }
 canvasGroup.alpha =0f;
 }
}
