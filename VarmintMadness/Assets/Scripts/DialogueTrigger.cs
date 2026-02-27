using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueSystem dialogueSystem;

    void Awake()
    {
        // Auto-find DialogueSystem on same GameObject if not assigned
        if (dialogueSystem == null)
        {
            dialogueSystem = GetComponent<DialogueSystem>();
        }
    }

    public void ShowDialogue()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.StartDialogue();
        }
        else
        {
            Debug.LogError("DialogueSystem not assigned to DialogueTrigger!");
        }
    }
}
