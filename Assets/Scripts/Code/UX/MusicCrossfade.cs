using UnityEngine;
using System.Collections;

public class MusicCrossfade : MonoBehaviour
{
    public AudioSource sourceA;
    public AudioSource sourceB;
    public AudioClip track1;
    public AudioClip track2;
    public float fadeTime = 2f;

    private AudioSource currentSource;
    private AudioSource nextSource;
    private int currentTrack = 0;

    void Start()
    {
        currentSource = sourceA;
        nextSource = sourceB;

        currentSource.clip = track1;
        currentSource.volume = 1f;
        currentSource.Play();

        StartCoroutine(CheckMusicEnd());
    }

    IEnumerator CheckMusicEnd()
    {
        while (true)
        {
            // Khi gần hết nhạc thì bắt đầu crossfade
            if (!currentSource.isPlaying)
            {
                PlayNextTrack();
            }
            yield return null;
        }
    }

    void PlayNextTrack()
    {
        // Chọn bài kế tiếp
        AudioClip nextClip = (currentTrack == 0) ? track2 : track1;
        currentTrack = 1 - currentTrack;

        // Chuẩn bị source mới
        nextSource.clip = nextClip;
        nextSource.volume = 0f;
        nextSource.Play();

        // Bắt đầu crossfade
        StartCoroutine(Crossfade());
    }

    IEnumerator Crossfade()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float progress = t / fadeTime;

            currentSource.volume = Mathf.Lerp(1f, 0f, progress);
            nextSource.volume = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        // Hoán đổi source
        var temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;
    }
}
