using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardSceneIdentifier : MonoBehaviour
{
    public bool isBoardScene = true;

    void Awake()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        BoardStateSaver.lastBoardSceneName = sceneName;

        Debug.Log("BoardSceneIdentifier: Registered board scene → " + sceneName);
    }
}
