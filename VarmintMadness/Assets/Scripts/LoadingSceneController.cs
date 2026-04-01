using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadBoardAsync());
    }

    IEnumerator LoadBoardAsync()
    {
        string boardScene = BoardStateSaver.lastBoardSceneName;

        if (string.IsNullOrEmpty(boardScene))
        {
            Debug.LogWarning("BoardStateSaver.lastBoardSceneName was empty! Using fallback.");
            boardScene = "Board 1"; // your default board
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(boardScene);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        yield return new WaitForSeconds(0.5f);

        asyncLoad.allowSceneActivation = true;
    }
}