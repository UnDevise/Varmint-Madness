using UnityEngine;
using System.Collections;

public class UltimateButtonDodger : MonoBehaviour
{
    [Header("UI Buttons")]
    public RectTransform playHitbox;
    public RectTransform quitHitbox;
    public RectTransform playVisuals;
    public RectTransform quitVisuals;

    [Header("Hover Settings")]
    public float hoverDelay = 0.5f;

    [Header("Jumpscare Setup")]
    public GameObject jumpscarePanel;
    public GameObject silentImage;
    public GameObject loudImage;
    public AudioSource scareSound;
    public AudioSource mainMusic;

    [Header("Randomized Silence")]
    public float minSilence = 1.0f; // Minimum wait time in seconds
    public float maxSilence = 5.0f; // Maximum wait time in seconds

    [Header("Scare Timing")]
    public float scareDuration = 1.5f;

    private Coroutine swapCoroutine;
    private bool isSwapped = false;
    private bool isQuitting = false;

    public void ExecutePlay()
    {
        if (isQuitting) return;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Local or multiplayer");
    }

    public void ExecuteQuit()
    {
        if (!isQuitting) StartCoroutine(JumpscareSequence());
    }

    private IEnumerator JumpscareSequence()
    {
        isQuitting = true;

        if (mainMusic != null) mainMusic.Stop();

        if (jumpscarePanel != null) jumpscarePanel.SetActive(true);
        if (silentImage != null) silentImage.SetActive(true);
        if (loudImage != null) loudImage.SetActive(false);

        // --- NEW RANDOM LOGIC ---
        // Generates a random float between your min and max values
        float randomWait = Random.Range(minSilence, maxSilence);
        yield return new WaitForSeconds(randomWait);
        // -------------------------

        if (silentImage != null) silentImage.SetActive(false);
        if (loudImage != null) loudImage.SetActive(true);
        if (scareSound != null) scareSound.Play();

        yield return new WaitForSeconds(scareDuration);

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- BUTTON DODGING LOGIC ---
    public void OnQuitHitboxClick() { if (isSwapped) ExecutePlay(); else ExecuteQuit(); }
    public void OnPlayHitboxClick() { if (isSwapped) ExecuteQuit(); else ExecutePlay(); }

    public void OnHoverEnter()
    {
        if (isSwapped || isQuitting) return;
        if (swapCoroutine != null) StopCoroutine(swapCoroutine);
        swapCoroutine = StartCoroutine(SwapAfterDelay());
    }

    public void OnHoverExit()
    {
        if (swapCoroutine != null) StopCoroutine(swapCoroutine);
        if (!isQuitting) ResetPositions();
    }

    private IEnumerator SwapAfterDelay()
    {
        yield return new WaitForSeconds(hoverDelay);
        playVisuals.localPosition = playVisuals.parent.InverseTransformPoint(quitHitbox.position);
        quitVisuals.localPosition = quitVisuals.parent.InverseTransformPoint(playHitbox.position);
        isSwapped = true;
    }

    private void ResetPositions()
    {
        playVisuals.localPosition = Vector3.zero;
        quitVisuals.localPosition = Vector3.zero;
        isSwapped = false;
    }
}
