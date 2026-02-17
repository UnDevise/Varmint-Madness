using UnityEngine;

public class MarbleSelector : MonoBehaviour
{
    public int marbleIndex; // Set in Inspector
    private GameManagerMarble manager;
    private SpriteRenderer sr;

    void Start()
    {
        manager = Object.FindAnyObjectByType<GameManagerMarble>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        // Only allow clicking if this marble is still available
        if (enabled)
        {
            manager.PlayerPickedMarble(this);
        }
    }

    // Called when this marble is chosen by a player
    public void EnableMarble()
    {
        gameObject.SetActive(true);
        enabled = true;

        if (sr != null)
            sr.color = Color.white;
    }

    // Called when this marble is NOT chosen
    public void HideUnpicked()
    {
        gameObject.SetActive(false);
    }

    // Called when another player picks this marble
    public void DisableMarble()
    {
        enabled = false;

        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0.4f);
    }
}


