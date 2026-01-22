using UnityEngine;

public class QuitManager : MonoBehaviour
{
    public void QuitGame()
    {
        // This will close the application in a built version
        Application.Quit();

        // Helpful for testing in the Unity Editor console
        Debug.Log("Game is exiting...");

        // (Optional) Stop Play Mode in the Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
