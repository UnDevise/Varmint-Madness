using UnityEngine;
using UnityEngine.Playables;

public class IntroCutsceneController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject dialogueBoxUI;

    public void PlayIntroCutscene()
    {
        // optional: hide dialogue when cutscene starts
        if (dialogueBoxUI != null)
            dialogueBoxUI.SetActive(false);

        director.Play();
    }
}
