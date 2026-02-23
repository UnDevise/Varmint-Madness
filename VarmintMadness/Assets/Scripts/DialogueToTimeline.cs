using UnityEngine;
using UnityEngine.Playables;

public class IntroCutsceneBridge : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    public void PlayBlinkCutscene()
    {
        Debug.Log("Dialogue closed → trying to play timeline: " + director.name);
        if (director == null) director = GetComponent<PlayableDirector>();
        if (director == null)
        {
            Debug.LogError("No PlayableDirector found for IntroCutsceneBridge.");
            return;
        }

        // Important: rewind so it starts from the beginning every time
        director.time = 0;
        director.Evaluate();   // forces the rewind to visually apply immediately
        director.Play();
    }
}