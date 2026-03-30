using UnityEngine;
using UnityEngine.UI;

public class MinigameIntroController : MonoBehaviour
{
    public Text titleText;
    public Text controlsText;
    public Image iconImage;

    public GameObject gameplayRoot; // The actual minigame logic root
    public MinigameInfo minigameInfo;

    void Start()
    {
        // Disable gameplay until intro is dismissed
        if (gameplayRoot != null)
            gameplayRoot.SetActive(false);

        ShowIntro();
    }

    public void ShowIntro()
    {
        gameObject.SetActive(true);

        if (minigameInfo != null)
        {
            titleText.text = minigameInfo.minigameName;
            controlsText.text = minigameInfo.controlsDescription;

            if (iconImage != null && minigameInfo.minigameIcon != null)
                iconImage.sprite = minigameInfo.minigameIcon;
        }
    }

    public void StartMinigame()
    {
        gameObject.SetActive(false);

        if (gameplayRoot != null)
            gameplayRoot.SetActive(true);
    }
}
