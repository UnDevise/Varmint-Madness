using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    private CharacterData[] characters;
    private int currentIndex = 0;

    [Header("UI Elements")]
    public Image characterImage;
    public Image previousCharacterImage;
    public Image nextCharacterImage;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterBioText;
    public Image backgroundPanel;

    [Header("Animation Settings")]
    public float transitionDuration = 0.3f;
    public Vector3 centerScale = Vector3.one;
    public Vector3 sideScale = new Vector3(0.5f, 0.5f, 1f);
    public Vector3 previousPosition = new Vector3(-300f, 0f, 0f);
    public Vector3 nextPosition = new Vector3(300f, 0f, 0f);
    public Vector3 centerPosition = Vector3.zero;

    void Start()
    {
        characters = Resources.LoadAll<CharacterData>("Characters");

        if (characters.Length > 0)
        {
            // FIX: Access the first character's color directly.
            // A coroutine is not needed for the initial color change.
            backgroundPanel.color = characters[0].backgroundColor;
            UpdateCharacterDisplays();
        }
        else
        {
            Debug.LogError("No CharacterData assets found in the 'Resources/Characters' folder!");
        }
    }

    public void NextCharacter()
    {
        if (characters.Length == 0) return;
        currentIndex = (currentIndex + 1) % characters.Length;
        StartCoroutine(AnimateTransition(1));
    }

    public void PreviousCharacter()
    {
        if (characters.Length == 0) return;
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = characters.Length - 1;
        }
        StartCoroutine(AnimateTransition(-1));
    }

    // Updates the sprites and text without animating
    void UpdateCharacterDisplays()
    {
        if (characters.Length == 0) return;

        // Update main character
        characterImage.sprite = characters[currentIndex].characterSprite;
        characterNameText.text = characters[currentIndex].characterName;
        characterBioText.text = characters[currentIndex].characterBio;

        // Update previous character preview
        int prevIndex = (currentIndex - 1 + characters.Length) % characters.Length;
        previousCharacterImage.sprite = characters[prevIndex].characterSprite;
        previousCharacterImage.rectTransform.localPosition = previousPosition;
        previousCharacterImage.rectTransform.localScale = sideScale;

        // Update next character preview
        int nextIndex = (currentIndex + 1) % characters.Length;
        nextCharacterImage.sprite = characters[nextIndex].characterSprite;
        nextCharacterImage.rectTransform.localPosition = nextPosition;
        nextCharacterImage.rectTransform.localScale = sideScale;
    }

    // Coroutine for handling all animations
    IEnumerator AnimateTransition(int direction)
    {
        // Animate the background color change
        StartCoroutine(FadeToNewColor());

        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            if (direction > 0) // Moving to next character
            {
                // Lerp current center character to previous position
                characterImage.rectTransform.localPosition = Vector3.Lerp(centerPosition, previousPosition, t);
                characterImage.rectTransform.localScale = Vector3.Lerp(centerScale, sideScale, t);

                // Lerp next character to center position
                nextCharacterImage.rectTransform.localPosition = Vector3.Lerp(nextPosition, centerPosition, t);
                nextCharacterImage.rectTransform.localScale = Vector3.Lerp(sideScale, centerScale, t);
            }
            else // Moving to previous character
            {
                // Lerp current center character to next position
                characterImage.rectTransform.localPosition = Vector3.Lerp(centerPosition, nextPosition, t);
                characterImage.rectTransform.localScale = Vector3.Lerp(centerScale, sideScale, t);

                // Lerp previous character to center position
                previousCharacterImage.rectTransform.localPosition = Vector3.Lerp(previousPosition, centerPosition, t);
                previousCharacterImage.rectTransform.localScale = Vector3.Lerp(sideScale, centerScale, t);
            }
            yield return null;
        }

        // Snap images to final positions and scales and update sprites
        UpdateCharacterDisplays();

        // This part isn't strictly necessary as UpdateCharacterDisplays already handles it,
        // but it ensures a clean state after the transition.
        characterImage.rectTransform.localPosition = centerPosition;
        characterImage.rectTransform.localScale = centerScale;
    }

    // Coroutine to smoothly fade the background to the new color
    IEnumerator FadeToNewColor()
    {
        Color startColor = backgroundPanel.color;
        Color targetColor = characters[currentIndex].backgroundColor;
        targetColor.a = 1f;

        float elapsedTime = 0;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            backgroundPanel.color = Color.Lerp(startColor, targetColor, elapsedTime / transitionDuration);
            yield return null;
        }

        backgroundPanel.color = targetColor;
    }
}
