using UnityEngine;
using System.Collections;

public class AudioFader : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 5f; // Adjust for fade

    void Start()
    {
        audioSource.volume = 0f;
        audioSource.Play();

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 0.5f, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0.3f; // Adjust for final volume
    }
}

