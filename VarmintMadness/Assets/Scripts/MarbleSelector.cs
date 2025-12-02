using UnityEngine;

public class MarbleSelector : MonoBehaviour
{
    public GameManagerMarble gameManager;

    private bool isSelected = false;

    void OnMouseDown()
    {
        if (!isSelected && gameManager != null)
        {
            isSelected = true; // Prevent double‑clicks
            gameManager.PlayerChoseMarble();
        }
    }
}

