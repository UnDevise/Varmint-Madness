using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Audio; // Required for AudioMixerGroup

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI mainDialogueText;
    public TextMeshProUGUI secondaryDialogueText;

    [Header("Audio Settings")]
    public AudioSource voiceAudioSource;
    public AudioMixerGroup outputGroup; // Drag your Mixer Group (e.g., Dialogue) here

    [System.Serializable]
    public struct DialogueEntry
    {
        [TextArea(3, 10)]
        public string mainText;
        [TextArea(3, 10)]
        public string secondaryText;
        public AudioClip voiceClip;
    }

    [Header("Sequence Settings")]
    public DialogueEntry[] dialogueLines;
    public float typingSpeed = 0.03f;
    public float punctuationPause = 0.5f;

    private int currentLineIndex = 0;
    private bool isTyping = false;

    void Start()
    {
        mainDialogueText.text = "";
        secondaryDialogueText.text = "";

        if (voiceAudioSource != null)
        {
            voiceAudioSource.loop = true;
            // Routes the AudioSource to your selected Mixer Group
            if (outputGroup != null)
            {
                voiceAudioSource.outputAudioMixerGroup = outputGroup;
            }
        }

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

        if (voiceAudioSource != null && currentClip != null)
        {
            voiceAudioSource.clip = currentClip;
        }

        foreach (char c in secondaryStr)
        {
            secondaryDialogueText.text += c;

            bool isPunctuation = IsPunctuation(c);
            float delay = isPunctuation ? punctuationPause : typingSpeed;

            if (voiceAudioSource != null && currentClip != null)
            {
                if (!isPunctuation && !voiceAudioSource.isPlaying)
                {
                    voiceAudioSource.Play();
                }
                else if (isPunctuation && voiceAudioSource.isPlaying)
                {
                    voiceAudioSource.Pause();
                }
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
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }
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
