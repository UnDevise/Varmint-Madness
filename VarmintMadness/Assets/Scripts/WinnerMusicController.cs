using UnityEngine;

public class WinnerThemePlayer : MonoBehaviour
{
    public static WinnerThemePlayer Instance;

    public AudioSource musicSource;
    public AudioClip[] characterThemes;
    // 0 = Chipmunk, 1 = Fox, 2 = Opossum, 3 = Raccoon

    private int winnerCharacter;
    private bool hasPlayed = false;

    void Awake()
    {
        Instance = this;
        winnerCharacter = PlayerPrefs.GetInt("WinnerCharacter", 0);
    }

    public void PlayWinnerTheme()
    {
        if (hasPlayed) return;

        if (winnerCharacter >= 0 && winnerCharacter < characterThemes.Length)
        {
            musicSource.clip = characterThemes[winnerCharacter];
            musicSource.Play();
            hasPlayed = true;
        }
        else
        {
            Debug.LogWarning("Winner character index out of range!");
        }
    }
}
