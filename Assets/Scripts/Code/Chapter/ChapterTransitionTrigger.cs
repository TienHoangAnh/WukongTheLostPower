using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ChapterTransitionTrigger : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Time needed to stand in the area to pass the map (seconds)")]
    public float requiredTime = 3f;

    private float timer = 0f;
    private bool isPlayerInZone = false;
    private bool isTransitioning = false;

    private void Update()
    {
        if (!isPlayerInZone || isTransitioning) return;

        timer += Time.deltaTime;

        if (timer >= requiredTime)
        {
            isTransitioning = true;
            StartCoroutine(TransitionToNextMap());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInZone = true;
        timer = 0f;
        Debug.Log("[Transition] Player entered zone.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInZone = false;
        timer = 0f;
        Debug.Log("[Transition] Player left zone.");
    }

    private System.Collections.IEnumerator TransitionToNextMap()
    {
        Debug.Log("[Transition] Changing scene...");

        if (ChapterManager.Instance == null)
        {
            Debug.LogError("[Transition] ChapterManager missing!");
            yield break;
        }

        if (!ChapterManager.Instance.HasNextMap())
        {
            Debug.Log("[Transition] Reached final map — show ending or credits.");
            yield break;
        }

        // Advance and get next map name
        ChapterManager.Instance.AdvanceMap();
        string nextScene = ChapterManager.Instance.GetNextMapName();

        // Optional: fade effect or delay
        yield return new WaitForSeconds(1f);

        Debug.Log($"[Transition] Loading next map: {nextScene}");
        SceneManager.LoadScene(nextScene);
    }
}
