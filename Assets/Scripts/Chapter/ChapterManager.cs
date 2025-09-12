using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public static ChapterManager Instance { get; private set; }

    [Header("Cài đặt chương")]
    public int currentChapter = 1;
    public int maxChapter = 5;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ lại khi chuyển scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GetNextChapterName()
    {
        return "Chapter" + currentChapter;
    }

    public bool HasNextChapter()
    {
        return currentChapter < maxChapter;
    }

    public void AdvanceChapter()
    {
        if (HasNextChapter())
        {
            currentChapter++;
        }
    }
}
