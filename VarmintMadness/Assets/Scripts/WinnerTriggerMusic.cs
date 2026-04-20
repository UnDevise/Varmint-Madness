using UnityEngine;

public class WinnerTriggerMusic : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip[] characterThemes;

    private bool hasPlayed = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasPlayed) return;

        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            int charIndex = pm.characterId;

            if (charIndex >= 0 && charIndex < characterThemes.Length)
            {
                musicSource.clip = characterThemes[charIndex];
                musicSource.Play();
                hasPlayed = true;
            }
        }
    }
}