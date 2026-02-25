using UnityEngine;
using UnityEngine.Playables;

public class IntroCutsceneBridge : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    public void PlayBlinkCutscene()
    {
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }
        
        if (director == null)
        {
            Debug.LogError("No PlayableDirector found for IntroCutsceneBridge. Make sure it's assigned in Inspector or on same GameObject.");
            return;
        }

        Debug.Log("Dialogue closed → playing timeline: " + director.name);
        
        // Important: rewind so it starts from the beginning every time
        director.time = 0;
        director.Evaluate();   // forces the rewind to visually apply immediately
        director.Play();
    }
}
