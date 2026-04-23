using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    private CharacterData2[] characters;
    private int currentIndex = 0;
    private bool isAnimating = false;
    private Coroutine backgroundFadeCoroutine;

    [Header("Multiplayer Settings")]
    public GameObject playerCountPanel;
    public TextMeshProUGUI statusText;
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;
    private int currentPlayer = 1;

    private int totalPlayersToSelect = 0;
    private List<int> lockedCharacterIndices = new List<int>();

    [Header("UI Elements")]
    public Image characterImage;
    public Image previousCharacterImage;
    public Image nextCharacterImage;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterBioText;
    public Image backgroundPanel;

    [Header("Smooth Animation Settings")]
    public float transitionDuration = 0.35f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public Vector3 centerScale = Vector3.one;
    public Vector3 sideScale = new Vector3(0.6f, 0.6f, 1f);
    public Vector3 previousPosition = new Vector3(-400f, 0f, 0f);
    public Vector3 nextPosition = new Vector3(400f, 0f, 0f);
    public Vector3 centerPosition = Vector3.zero;

    void Start()
    {
        characters = Resources.LoadAll<CharacterData2>("Characters");
        playerCountPanel.SetActive(true);

        if (characters.Length > 0)
        {
            UpdateCharacterDisplays();
            backgroundPanel.color = characters[currentIndex].backgroundColor;
        }
    }

    public void SetPlayerCount(int count)
    {
        totalPlayersToSelect = count;
        playerCountPanel.SetActive(false);
        UpdateStatusUI();
    }

    // SELECT BUTTON LOGIC
    public void OnActionButtonPressed()
    {
        if (totalPlayersToSelect == 0) return;

        // If players are still selecting
        if (lockedCharacterIndices.Count < totalPlayersToSelect)
        {
            if (!lockedCharacterIndices.Contains(currentIndex))
            {
                lockedCharacterIndices.Add(currentIndex);
                Debug.Log($"Player {currentPlayer} selected index {currentIndex}");

                currentPlayer++;  // ⭐ Move to next player

                UpdateCharacterDisplays();
            }
}
        else
        {
            // --- SAVE DATA FOR BOARD SCENE ---
            PlayerPrefs.SetInt("TotalPlayers", totalPlayersToSelect);

            // Save only the number of players actually selected
            for (int i = 0; i < lockedCharacterIndices.Count; i++)
            {
                PlayerPrefs.SetInt($"P{i + 1}_Character", lockedCharacterIndices[i]);
                Debug.Log($"Saved P{i + 1}_Character = {lockedCharacterIndices[i]}");
            }

            PlayerPrefs.Save();

            // Reset board state so garbage doesn't get overwritten
            BoardStateSaver.Clear();
            BoardStateSaver.returningFromMinigame = false;

            // --- END SAVE ---

            SceneManager.LoadScene("Board Picker");
        }
    }

    public void NextCharacter()
    {
        if (isAnimating || characters.Length == 0) return;
        currentIndex = (currentIndex + 1) % characters.Length;
        StartCoroutine(AnimateTransition(1));
    }

    public void PreviousCharacter()
    {
        if (isAnimating || characters.Length == 0) return;
        currentIndex = (currentIndex - 1 + characters.Length) % characters.Length;
        StartCoroutine(AnimateTransition(-1));
    }

    void UpdateCharacterDisplays()
    {
        if (characters.Length == 0) return;

        Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        characterImage.sprite = characters[currentIndex].characterSprite;
        characterNameText.text = characters[currentIndex].characterName;
        characterBioText.text = characters[currentIndex].characterBio;
        characterImage.color = lockedCharacterIndices.Contains(currentIndex) ? lockedColor : Color.white;

        int prevIndex = (currentIndex - 1 + characters.Length) % characters.Length;
        previousCharacterImage.sprite = characters[prevIndex].characterSprite;
        previousCharacterImage.color = lockedCharacterIndices.Contains(prevIndex) ? lockedColor : Color.white;

        int nextIndex = (currentIndex + 1) % characters.Length;
        nextCharacterImage.sprite = characters[nextIndex].characterSprite;
        nextCharacterImage.color = lockedCharacterIndices.Contains(nextIndex) ? lockedColor : Color.white;

        UpdateStatusUI();
    }

    void UpdateStatusUI()
    {
        if (totalPlayersToSelect == 0) return;

        if (lockedCharacterIndices.Count < totalPlayersToSelect)
        {
            statusText.text = "Player " + currentPlayer + " Selecting...";
            bool isTaken = lockedCharacterIndices.Contains(currentIndex);
            actionButtonText.text = isTaken ? "TAKEN" : "SELECT";
            actionButton.interactable = !isTaken;
        }
        else
        {
            statusText.text = "Ready to Play!";
            actionButtonText.text = "PLAY";
            actionButton.interactable = true;
        }
    }

    IEnumerator AnimateTransition(int direction)
    {
        isAnimating = true;
        if (backgroundFadeCoroutine != null) StopCoroutine(backgroundFadeCoroutine);
        backgroundFadeCoroutine = StartCoroutine(FadeBackground());

        float elapsed = 0;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = transitionCurve.Evaluate(elapsed / transitionDuration);

            if (direction > 0)
            {
                characterImage.rectTransform.localPosition = Vector3.Lerp(centerPosition, previousPosition, t);
                nextCharacterImage.rectTransform.localPosition = Vector3.Lerp(nextPosition, centerPosition, t);
            }
            else
            {
                characterImage.rectTransform.localPosition = Vector3.Lerp(centerPosition, nextPosition, t);
                previousCharacterImage.rectTransform.localPosition = Vector3.Lerp(previousPosition, centerPosition, t);
            }
            yield return null;
        }

        UpdateCharacterDisplays();
        characterImage.rectTransform.localPosition = centerPosition;
        previousCharacterImage.rectTransform.localPosition = previousPosition;
        nextCharacterImage.rectTransform.localPosition = nextPosition;
        isAnimating = false;
    }

    IEnumerator FadeBackground()
    {
        Color startColor = backgroundPanel.color;
        Color targetColor = characters[currentIndex].backgroundColor;
        float elapsed = 0;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            backgroundPanel.color = Color.Lerp(startColor, targetColor, transitionCurve.Evaluate(elapsed / transitionDuration));
            yield return null;
        }
        backgroundPanel.color = targetColor;
    }

}