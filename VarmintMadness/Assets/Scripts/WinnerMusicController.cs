using UnityEngine;

public class WinnerThemePlayer : MonoBehaviour
{
    public static WinnerThemePlayer Instance;

    public AudioSource musicSource;
    public AudioClip[] characterThemes;
    // 0 = Chipmunk, 1 = Fox, 2 = Opossum, 3 = Raccoon

    private bool hasPlayed = false;

    void Awake()
    {
        Instance = this;
    }

    public void PlayWinnerTheme()
    {
        if (hasPlayed) return;

        string winnerName = WinnerData.WinnerName;

        Debug.Log("Winner name is: " + winnerName);

        // Normalize and extract first word
        winnerName = winnerName.Trim().ToLower();
        string[] parts = winnerName.Split(' ');
        string characterKey = parts[0]; // "chipmunk" from "Chipmunk Bella"

        switch (characterKey)
        {
            case "chipmunk":
                musicSource.clip = characterThemes[0];
                break;

            case "fox":
                musicSource.clip = characterThemes[1];
                break;

            case "opossum":
                musicSource.clip = characterThemes[2];
                break;

            case "raccoon":
                musicSource.clip = characterThemes[3];
                break;

            default:
                Debug.LogWarning("Unknown winner character key: " + characterKey);
                return;
        }

        musicSource.Play();
        hasPlayed = true;
    }
}