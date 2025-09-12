using UnityEngine;

public class MusicLooper : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip track1;
    public AudioClip track2;

    private int currentTrack = 0;

    void Start()
    {
        PlayNextTrack();
    }

    void Update()
    {
        // Khi nhạc phát xong thì chuyển bài
        if (!audioSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    void PlayNextTrack()
    {
        if (currentTrack == 0)
        {
            audioSource.clip = track1;
            currentTrack = 1;
        }
        else
        {
            audioSource.clip = track2;
            currentTrack = 0;
        }
        audioSource.Play();
    }
}
