using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider dialogueSlider; // Added dialogue slider reference

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        myMixer.SetFloat("SFXVol", Mathf.Log10(volume) * 20);
    }

    // New function for Dialogue
    public void SetDialogueVolume()
    {
        float volume = dialogueSlider.value;
        // Ensure your Dialogue group's volume is exposed as "DialogueVol"
        myMixer.SetFloat("DialogueVol", Mathf.Log10(volume) * 20);
    }
}
