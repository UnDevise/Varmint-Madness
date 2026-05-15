using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Delay Before Loading")]
    public float preLoadDelay = 2f;

    [Header("Error Message UI")]
    public TextMeshProUGUI errorText;

    [Header("Error Display Duration")]
    public float errorDisplayTime = 3f;

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

            yield return new WaitForSeconds(errorDisplayTime);
        }

        // ⭐ APPLY MINIGAME REWARD (if any)
        // Minigames should set MarbleRewardData.WinnerPlayerIndices and BonusTrash.
        // If they didn't, this safely does nothing.
        if (MarbleRewardData.WinnerPlayerIndices != null &&
            MarbleRewardData.WinnerPlayerIndices.Count > 0)
        {
            Debug.Log("Applying minigame reward: +" + MarbleRewardData.BonusTrash + " trash to player index " +
                      MarbleRewardData.WinnerPlayerIndices[0]);
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