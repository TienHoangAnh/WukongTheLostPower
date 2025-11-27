using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UI_Toasts : MonoBehaviour
{
    public static UI_Toasts Instance;
    public TextMeshProUGUI toastText;
    public CanvasGroup canvasGroup;
    public float showDuration = 1.5f;

    void Awake() { Instance = this; }

    public static void Show(string message)
    {
        if (Instance != null)
            Instance.StartCoroutine(Instance.ShowToast(message));
        else
            Debug.Log("[Toast] " + message);
    }

    IEnumerator ShowToast(string msg)
    {
        toastText.text = msg;
        StopAllCoroutines();
        StartCoroutine(FadeInOut());
        yield return null;
    }

    IEnumerator FadeInOut()
    {
        canvasGroup.alpha = 1;
        yield return new WaitForSeconds(showDuration);
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime;
            canvasGroup.alpha = t;
            yield return null;
        }
    }
}
