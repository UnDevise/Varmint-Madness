using UnityEngine;

public class MiniGameFinishBlock : MonoBehaviour
{
    public MiniGameMusic musicPlayer;
    private bool hasPlayedFinishMusic = false;

    private GameManagerMarble manager;

    private void Start()
    {
        manager = FindObjectOfType<GameManagerMarble>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Marble"))
            return;

        // Play finish music once
        if (!hasPlayedFinishMusic)
        {
            if (musicPlayer != null)
            {
                musicPlayer.PlayFinishSong();
                hasPlayedFinishMusic = true;
            }
        }

        // Identify which marble finished
        MarbleMovement marble = collision.GetComponent<MarbleMovement>();

        if (marble != null)
        {
            manager.MarbleReachedFinish(marble.marbleIndex);
        }
    }
}



