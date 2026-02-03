using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerModeSelector : MonoBehaviour
{
    public string localSceneName = "LocalMultiplayerScene";
    public string onlineSceneName = "OnlineLobbyScene";

    public void PlayLocal()
    {
        Debug.Log("Starting Local Multiplayer");
        SceneManager.LoadScene(localSceneName);
    }

    public void PlayOnline()
    {
        Debug.Log("Starting Online Multiplayer");
        SceneManager.LoadScene(onlineSceneName);
    }
}
