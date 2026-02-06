using UnityEngine;

public class MarbleSelector : MonoBehaviour
{
    public int marbleIndex; // Set in Inspector
    private GameManagerMarble manager;

    void Start()
    {
        manager = FindObjectOfType<GameManagerMarble>();
    }

    void OnMouseDown()
    {
        // Only allow clicking if the marble is still available
        if (enabled)
        {
            manager.PlayerPickedMarble(this);
        }
    }

    public void DisableMarble()
    {
        // Disable clicking
        enabled = false;

        // Optional: fade the marble or change color to show it's taken
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.4f);
    }
}


