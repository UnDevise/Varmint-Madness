using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{

    public TextMeshProUGUI dialogueText;

    
    public string[] dialogueLines;
    public float typingSpeed = 0.03f; // Adjustable Speed

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
            yield return new WaitForSeconds(typingSpeed);
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

