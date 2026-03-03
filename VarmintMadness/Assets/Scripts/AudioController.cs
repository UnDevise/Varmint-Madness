using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip recordScratchClip;
    [SerializeField] private float scratchDelay = 0.5f; // Adjust this for timing

    public void StopMusicWithScratch()
    {
        StartCoroutine(StopMusicWithDelay());
    }

    private IEnumerator StopMusicWithDelay()
    {
        // Stop the background music immediately
        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        // Wait for the delay
        yield return new WaitForSeconds(scratchDelay);

        // Play the record scratch sound
        if (sfxSource != null && recordScratchClip != null)
        {
            sfxSource.PlayOneShot(recordScratchClip);
        }
    }
}
