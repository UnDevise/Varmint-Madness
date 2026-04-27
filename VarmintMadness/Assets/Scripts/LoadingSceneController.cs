using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Delay Before Loading")]
    public float preLoadDelay = 2f;

    [Header("Error Message UI")]
    public TextMeshProUGUI errorText;

    [Header("Error Display Duration")]
    public float errorDisplayTime = 3f;   // ⭐ How long the error stays visible

    void Start()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false);

        StartCoroutine(LoadBoardAsync());
    }

    IEnumerator LoadBoardAsync()
    {
        // Initial delay before anything happens
        yield return new WaitForSeconds(preLoadDelay);

        string boardScene = BoardStateSaver.lastBoardSceneName;

        bool usingFallback = false;

        if (string.IsNullOrEmpty(boardScene))
        {
            Debug.LogWarning("BoardStateSaver.lastBoardSceneName was empty! Using fallback.");
            boardScene = "Board 1"; // fallback board
            usingFallback = true;

            if (errorText != null)
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Error: This board has made a terrible mistake.\nLoading backup board...";
            }

            // ⭐ Wait so the player can read the message
            yield return new WaitForSeconds(errorDisplayTime);
        }

        // Begin loading the board
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(boardScene);
        asyncLoad.allowSceneActivation = false;

        // Wait until Unity finishes loading (90% = ready)
        while (asyncLoad.progress < 0.9f)
            yield return null;

        // Small buffer to ensure board initializes
        yield return new WaitForSeconds(0.5f);

        asyncLoad.allowSceneActivation = true;
    }
}