using UnityEngine;

public class MiniGameFinishBlock : MonoBehaviour
{
    public MiniGameMusic musicPlayer; // Drag MusicManager here in Inspector

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("FinishBlock triggered by: " + collision.name);

        // If you want tag check, make sure marbles are tagged "Marble"
        if (collision.CompareTag("Marble"))
        {
            if (musicPlayer != null)
            {
                Debug.Log("Playing finish song!");
                musicPlayer.PlayFinishSong();
            }
            else
            {
                Debug.LogWarning("MusicPlayer reference is missing!");
            }
        }
    }
}


