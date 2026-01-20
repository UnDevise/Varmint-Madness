using UnityEngine;
using System.Collections;

public class AudioFader : MonoBehaviour
{
    public AudioSource audioSource;
    public float fadeDuration = 2f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        audioSource.volume = 0f;
        audioSource.Play();

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = 1f;
    }
}

