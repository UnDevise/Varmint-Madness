using UnityEngine;

public class ComingSoonPopup : MonoBehaviour
{
    public GameObject comingSoonPanel;

    public void ShowComingSoon()
    {
        comingSoonPanel.SetActive(true);
    }

    public void HideComingSoon()
    {
        comingSoonPanel.SetActive(false);
    }
}
