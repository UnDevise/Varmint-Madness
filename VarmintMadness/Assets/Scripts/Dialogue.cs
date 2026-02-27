using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.Events;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI mainDialogueText;
    public TextMeshProUGUI secondaryDialogueText;

    [Header("Audio Settings")]
    public AudioSource voiceAudioSource;
    public AudioMixerGroup outputGroup;

    [System.Serializable]
    public struct DialogueEntry
    {
        [TextArea(3, 10)]
        public string mainText;
        [TextArea(3, 10)]
        public string secondaryText;
        public AudioClip voiceClip;

        [Header("Post-Dialogue Event")]
        public bool triggerHideBox; // Check this for the event-triggering line
        public float postEventDelay;       // Seconds to wait after box disappears
        public UnityEvent onBoxClosed;     // Event (Timeline) to play
    }

    [Header("Sequence Settings")]
    public DialogueEntry[] dialogueLines;
    public float typingSpeed = 0.03f;
    public float punctuationPause = 0.5f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueFinished = false;
    private bool dialogueActive = false; // Only accept input when dialogue is active

    void Start()
    {
        if (voiceAudioSource != null)
        {
            voiceAudioSource.loop = true;
            if (outputGroup != null) voiceAudioSource.outputAudioMixerGroup = outputGroup;
        }

        // Don't auto-start anymore - wait for signal
        dialogueBox.SetActive(false);
    }

    public void StartDialogue()
    {
        if (dialogueLines.Length > 0)
        {
            dialogueActive = true; // Enable input
            dialogueBox.SetActive(true);
            StartCoroutine(TypeLine());
        }
    }

    void Update()
    {
        if (!dialogueActive || dialogueFinished) return; // Only listen when dialogue is active and not finished
        
        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                FinishLineInstantly();
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
        mainDialogueText.text = dialogueLines[currentLineIndex].mainText;
        secondaryDialogueText.text = "";

        string secondaryStr = dialogueLines[currentLineIndex].secondaryText;
        AudioClip currentClip = dialogueLines[currentLineIndex].voiceClip;

        if (voiceAudioSource != null && currentClip != null) voiceAudioSource.clip = currentClip;

        foreach (char c in secondaryStr)
        {
            secondaryDialogueText.text += c;
            bool isPunctuation = IsPunctuation(c);
            float delay = isPunctuation ? punctuationPause : typingSpeed;

            if (voiceAudioSource != null && currentClip != null)
            {
                if (!isPunctuation && !voiceAudioSource.isPlaying) voiceAudioSource.Play();
                else if (isPunctuation && voiceAudioSource.isPlaying) voiceAudioSource.Pause();
            }
            yield return new WaitForSeconds(delay);
        }

        StopAudio();
        isTyping = false;
    }

    void FinishLineInstantly()
    {
        secondaryDialogueText.text = dialogueLines[currentLineIndex].secondaryText;
        StopAudio();
        isTyping = false;
    }

    void StopAudio()
    {
        if (voiceAudioSource != null && voiceAudioSource.isPlaying) voiceAudioSource.Stop();
    }

    bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' || c == ':' || c == ';';
    }

    void NextLine()
    {
        // If this line is set to trigger an event, start the closure sequence
        if (dialogueLines[currentLineIndex].triggerHideBox)
        {
            dialogueFinished = true; // Stop accepting input
            // Hide box first
            dialogueBox.SetActive(false);
            Debug.Log("Dialogue box closed, triggering event after " + dialogueLines[currentLineIndex].postEventDelay + " seconds...");
            
            // Invoke after delay without coroutine (in case this script is on the box itself)
            Invoke(nameof(TriggerEvent), dialogueLines[currentLineIndex].postEventDelay);
            return;
        }

        // Otherwise, move to next line normally
        if (currentLineIndex < dialogueLines.Length - 1)
        {
            currentLineIndex++;
            StartCoroutine(TypeLine());
        }
        else
        {
            dialogueFinished = true; // Stop accepting input
            // Auto-trigger if it's the absolute last line
            dialogueBox.SetActive(false);
            Invoke(nameof(TriggerEvent), dialogueLines[currentLineIndex].postEventDelay);
        }
    }

    void TriggerEvent()
    {
        Debug.Log("Invoking onBoxClosed event now! Event listener count: " + dialogueLines[currentLineIndex].onBoxClosed.GetPersistentEventCount());
        dialogueLines[currentLineIndex].onBoxClosed?.Invoke();
    }
}
