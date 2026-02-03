using UnityEngine;

public class MiniGameFinishBlock : MonoBehaviour
{
    public MiniGameMusic musicPlayer; // Drag MusicManager here in Inspector

    private bool hasPlayedFinishMusic = false; // NEW FLAG

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("FinishBlock triggered by: " + collision.name);

        if (collision.CompareTag("Marble"))
        {
            if (!hasPlayedFinishMusic) // Only play once
            {
                if (musicPlayer != null)
                {
                    Debug.Log("Playing finish song!");
                    musicPlayer.PlayFinishSong();
                    hasPlayedFinishMusic = true; // Prevent future plays
                }
                else
                {
                    Debug.LogWarning("MusicPlayer reference is missing!");
                }
            }
            else
            {
                Debug.Log("Finish music already played, ignoring.");
            }
        }
    }
}


