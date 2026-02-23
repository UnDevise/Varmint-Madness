using UnityEngine;

public class MarbleSelector : MonoBehaviour
{
    public int marbleIndex;
    private GameManagerMarble manager;
    private SpriteRenderer sr;

    void Start()
    {
        manager = Object.FindAnyObjectByType<GameManagerMarble>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (enabled)
            manager.PlayerPickedMarble(this);
    }

    public void EnableMarble()
    {
        gameObject.SetActive(true);
        enabled = true;

        if (sr != null)
            sr.color = Color.white;
    }

    public void HideUnpicked()
    {
        if (enabled)
            gameObject.SetActive(false);
    }

    public void DisableMarble()
    {
        enabled = false;

        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0.4f);
    }
}
