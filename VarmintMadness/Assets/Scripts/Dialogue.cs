using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;

    public string[] dialogueLines;
    public float typingSpeed = 0.03f; // Adjustable Speed
    public float punctuationPause = 1f; // Adjustable pause for punctuation

    private int currentLineIndex = 0;
    private bool isTyping = false;

    void Start()
    {
        dialogueText.text = "";
        StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[currentLineIndex];
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
        dialogueText.text = "";

        foreach (char c in dialogueLines[currentLineIndex])
        {
            dialogueText.text += c;

            float delay = typingSpeed;

            if (c == '.' || c == ',' || c == '!' || c == '?' || c == ':' || c == ';')
            {
                delay = punctuationPause;
            }

            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
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
            dialogueText.text = "";
            Debug.Log("Dialogue finished.");
        }
    }
}