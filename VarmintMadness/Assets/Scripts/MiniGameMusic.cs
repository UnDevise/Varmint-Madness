using UnityEngine;

public class MiniGameMusic : MonoBehaviour
{
    public AudioClip[] songs;       // Background songs
    public AudioClip finishSong;    // Special finish song
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (songs.Length > 0)
        {
            PlayRandomSong();
        }
    }

    public void PlayRandomSong()
    {
        int randomIndex = Random.Range(0, songs.Length);
        audioSource.clip = songs[randomIndex];
        audioSource.loop = true;
        audioSource.Play();
    }

    public void PlayFinishSong()
    {
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = finishSong;
        audioSource.Play();
    }
}


