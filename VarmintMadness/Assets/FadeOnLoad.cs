using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeOut : MonoBehaviour
{
    public float duration = 2.0f;
    private Image fadeImage;

    void Start()
    {
        fadeImage = GetComponent<Image>();
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color startColor = fadeImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            fadeImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Optional: Disable the object after fading
        gameObject.SetActive(false);
    }
}
