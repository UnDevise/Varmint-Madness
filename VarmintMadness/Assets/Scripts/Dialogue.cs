using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI mainDialogueText;      // This text appears instantly
    public TextMeshProUGUI secondaryDialogueText; // This text types out letter by letter

    [System.Serializable]
    public struct DialogueEntry
    {
        [TextArea(3, 10)]
        public string mainText;
        [TextArea(3, 10)]
        public string secondaryText;
    }

    [Header("Settings")]
    public DialogueEntry[] dialogueLines;
    public float typingSpeed = 0.03f;
    public float punctuationPause = 0.5f;

    private int currentLineIndex = 0;
    private bool isTyping = false;

    void Start()
    {
        // Initial clear
        mainDialogueText.text = "";
        secondaryDialogueText.text = "";

        if (dialogueLines.Length > 0)
        {
            StartCoroutine(TypeLine());
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Skip the typing effect for the secondary text
                StopAllCoroutines();
                secondaryDialogueText.text = dialogueLines[currentLineIndex].secondaryText;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;

        // 1. Set Main Text instantly (Static)
        mainDialogueText.text = dialogueLines[currentLineIndex].mainText;

        // 2. Clear and then type out Secondary Text
        secondaryDialogueText.text = "";
        string secondaryStr = dialogueLines[currentLineIndex].secondaryText;

        foreach (char c in secondaryStr)
        {
            secondaryDialogueText.text += c;

            float delay = typingSpeed;
            if (IsPunctuation(c))
            {
                delay = punctuationPause;
            }

            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
    }

    bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' || c == ':' || c == ';';
    }

    void NextLine()
    {
        if (currentLineIndex < dialogueLines.Length - 1)
        {
            currentLineIndex++;
            StartCoroutine(TypeLine());
        }
        else
        {
            mainDialogueText.text = "";
            secondaryDialogueText.text = "";
            Debug.Log("Dialogue finished.");
        }
    }
}