using UnityEngine;

public class TextToSpeech : MonoBehaviour
{
    public void Speak(string text)
    {
        Debug.Log("[TTS] " + text);
        // Later: replace this with a real TTS plugin call
    }
}