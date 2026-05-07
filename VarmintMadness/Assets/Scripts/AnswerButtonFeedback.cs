using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// AnswerButtonFeedback — Attach this to each of the 4 answer buttons.
/// Provides hover scaling and a brief flash of green (correct) or red (wrong)
/// after an answer is submitted.  Works automatically alongside GameShowManager.
/// </summary>
[RequireComponent(typeof(Button))]
public class AnswerButtonFeedback : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  INSPECTOR SETTINGS
    // ─────────────────────────────────────────────────────────────────────────

    [Header("── Hover Effect ──")]
    [Tooltip("Scale multiplier applied while the pointer is over the button.")]
    [Range(1f, 1.3f)]
    public float hoverScale = 1.08f;

    [Tooltip("Speed of the scale lerp.")]
    [Range(5f, 30f)]
    public float scaleSpeed = 12f;

    [Header("── Result Flash ──")]
    [Tooltip("Colour flashed when this button was chosen and the answer is correct.")]
    public Color correctColor = new Color(0.18f, 0.80f, 0.44f); // green

    [Tooltip("Colour flashed when this button was chosen and the answer is wrong.")]
    public Color wrongColor = new Color(0.90f, 0.25f, 0.25f);   // red

    [Tooltip("Colour flashed on the button that held the correct answer (shown to all).")]
    public Color revealColor = new Color(0.18f, 0.80f, 0.44f, 0.45f);

    [Tooltip("Duration of the result flash in seconds.")]
    [Range(0.3f, 3f)]
    public float flashDuration = 1.2f;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────────────────────────────────

    private Button btn;
    private Image btnImage;
    private Vector3 baseScale;
    private bool isHovered = false;
    private Color normalColor;

    // ─────────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        btn = GetComponent<Button>();
        btnImage = GetComponent<Image>();
        baseScale = transform.localScale;

        if (btnImage != null)
            normalColor = btnImage.color;
    }

    private void Update()
    {
        // Smooth hover scale
        Vector3 target = isHovered ? baseScale * hoverScale : baseScale;
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * scaleSpeed);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HOVER EVENTS  (called by EventTrigger component or Unity UI event system)
    // ─────────────────────────────────────────────────────────────────────────

    public void OnPointerEnter() => isHovered = true;
    public void OnPointerExit()  => isHovered = false;

    // ─────────────────────────────────────────────────────────────────────────
    //  RESULT FLASH  (called by GameShowManager after an answer is chosen)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Call this on every answer button after the player chooses.
    /// • wasChosen    = true on the button the player actually clicked.
    /// • wasCorrect   = whether that chosen button was the right answer.
    /// • isCorrectBtn = true on whichever button holds the correct answer.
    /// </summary>
    public void FlashResult(bool wasChosen, bool wasCorrect, bool isCorrectBtn)
    {
        StopAllCoroutines();
        StartCoroutine(DoFlash(wasChosen, wasCorrect, isCorrectBtn));
    }

    private IEnumerator DoFlash(bool wasChosen, bool wasCorrect, bool isCorrectBtn)
    {
        if (btnImage == null) yield break;

        isHovered = false;

        Color flashColor;

        if (wasChosen)
            flashColor = wasCorrect ? correctColor : wrongColor;
        else if (isCorrectBtn && !wasCorrect)
            flashColor = revealColor;   // reveal the right answer on a wrong guess
        else
            yield break;               // unrelated button — no flash

        // Instant to flash colour
        btnImage.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        // Fade back to normal
        float elapsed = 0f;
        float fadeDuration = 0.4f;
        while (elapsed < fadeDuration)
        {
            btnImage.color = Color.Lerp(flashColor, normalColor, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        btnImage.color = normalColor;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  RESET  (call when recycling the button for the next question)
    // ─────────────────────────────────────────────────────────────────────────

    public void ResetVisual()
    {
        StopAllCoroutines();
        isHovered = false;
        if (btnImage != null) btnImage.color = normalColor;
        transform.localScale = baseScale;
    }
}
