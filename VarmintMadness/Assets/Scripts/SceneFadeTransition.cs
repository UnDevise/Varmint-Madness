using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFadeTransition : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private string nextSceneName;

    public void FadeToNextScene()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        Color fadeColor = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = fadeColor;
            yield return null;
        }

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }
}
