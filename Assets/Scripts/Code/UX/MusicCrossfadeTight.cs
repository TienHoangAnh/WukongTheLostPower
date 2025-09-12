using UnityEngine;
using System.Collections;

public class MusicCrossfadeTight : MonoBehaviour
{
    public static MusicCrossfadeTight Instance;

    [Header("Audio Sources")]
    public AudioSource sourceA;
    public AudioSource sourceB;

    [Header("Clips")]
    public AudioClip track1;
    public AudioClip track2;

    [Header("Fade")]
    public float fadeTime = 2f;

    private AudioSource currentSource;
    private AudioSource nextSource;
    private AudioClip currentClip;
    private AudioClip nextClip;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        foreach (var s in new[] { sourceA, sourceB })
        {
            s.playOnAwake = false;
            s.loop = false;
            s.volume = 0f;
            s.Stop();
        }

        currentSource = sourceA;
        nextSource = sourceB;

        currentClip = track1;
        nextClip = track2;

        currentSource.clip = currentClip;
        currentSource.volume = 1f;
        currentSource.Play();

        StartCoroutine(Orchestrate());
    }

    IEnumerator Orchestrate()
    {
        while (true)
        {
            float safeFade = Mathf.Clamp(
                fadeTime,
                0.1f,
                Mathf.Max(0.1f, currentSource.clip.length - 0.1f)
            );

            yield return new WaitUntil(() =>
                currentSource.isPlaying &&
                (currentSource.clip.length - currentSource.time) <= safeFade
            );

            nextSource.clip = nextClip;
            nextSource.time = 0f;
            nextSource.volume = 0f;
            nextSource.Play();

            float t = 0f;
            float startVol = currentSource.volume;
            while (t < safeFade)
            {
                t += Time.deltaTime;
                float k = t / safeFade;
                currentSource.volume = Mathf.Lerp(startVol, 0f, k);
                nextSource.volume = Mathf.Lerp(0f, 1f, k);
                yield return null;
            }

            currentSource.volume = 0f;
            nextSource.volume = 1f;
            currentSource.Stop();

            var tmpS = currentSource; currentSource = nextSource; nextSource = tmpS;
            var tmpC = currentClip; currentClip = nextClip; nextClip = tmpC;
        }
    }
}
