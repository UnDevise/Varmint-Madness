using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene switching

public class SceneSwitcher : MonoBehaviour
{
    // Call this to load a scene by its exact name
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Call this to go back to the previous index (e.g., from Scene 1 to Scene 0)
    public void GoBack()
    {
        int previousSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        SceneManager.LoadScene(previousSceneIndex);
    }
}
