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

        // ⭐ ADD THIS LINE
        Debug.Log("Winner name is: " + winnerName);

        switch (winnerName)
        {
            case "Chipmunk":
                musicSource.clip = characterThemes[0];
                break;

            case "Fox":
                musicSource.clip = characterThemes[1];
                break;

            case "Opossum":
                musicSource.clip = characterThemes[2];
                break;

            case "Raccoon":
                musicSource.clip = characterThemes[3];
                break;

            default:
                Debug.LogWarning("Unknown winner name: " + winnerName);
                return;
        }

        musicSource.Play();
        hasPlayed = true;
    }

}