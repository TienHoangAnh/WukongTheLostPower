using UnityEngine;

public class ChapterManager : MonoBehaviour
{
    public static ChapterManager Instance { get; private set; }

    [Header("Set Size Chapter")]
    public int currentChapter = 1;
    public int maxChapter = 4;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
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
        return currentChapter <= maxChapter;
    }

    public void AdvanceChapter()
    {
        if (HasNextChapter())
        {
            currentChapter++;
        }
    }
}
