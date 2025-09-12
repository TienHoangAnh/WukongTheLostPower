using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterTransitionTrigger : MonoBehaviour
{
    public float requiredTime = 5f;
    private float timer = 0f;
    private bool isPlayerInZone = false;

    private void Update()
    {
        if (isPlayerInZone)
        {
            timer += Time.deltaTime;
            if (timer >= requiredTime)
            {
                Debug.Log("🌀 Đang chuyển scene...");

                // Tăng chapter và load
                ChapterManager.Instance.AdvanceChapter();
                string nextScene = ChapterManager.Instance.GetNextChapterName();
                SceneManager.LoadScene(nextScene);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger something"); // thử nghiệm
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            timer = 0f;
            Debug.Log("🟢 Player vào vùng chuyển cảnh.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            timer = 0f;
            Debug.Log("⚪ Player rời khỏi vùng.");
        }
    }
}
