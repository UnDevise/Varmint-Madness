using UnityEngine;
using System.Collections;

public class UltimateButtonDodger : MonoBehaviour
{
    public RectTransform playHitbox;
    public RectTransform quitHitbox;
    public RectTransform playVisuals;
    public RectTransform quitVisuals;

    public float hoverDelay = 0.5f;
    private Coroutine swapCoroutine;
    private bool isSwapped = false;

    // --- BUTTON ACTIONS ---
    public void ExecutePlay()
    {
        Debug.Log("PLAYING GAME...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("CharacterSelect");
    }

    public void ExecuteQuit()
    {
        Debug.Log("QUITTING GAME...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // --- CLICK ROUTING ---
    // Assigned to the QUIT HITBOX
    public void OnQuitHitboxClick()
    {
        if (isSwapped) ExecutePlay(); // If swapped, clicking here is actually clicking Play
        else ExecuteQuit();
    }

    // Assigned to the PLAY HITBOX
    public void OnPlayHitboxClick()
    {
        if (isSwapped) ExecuteQuit(); // If swapped, clicking here is actually clicking Quit
        else ExecutePlay();
    }

    // --- HOVER LOGIC ---
    public void OnHoverEnter()
    {
        if (isSwapped) return; // Prevent loop if already swapped
        if (swapCoroutine != null) StopCoroutine(swapCoroutine);
        swapCoroutine = StartCoroutine(SwapAfterDelay());
    }

    public void OnHoverExit()
    {
        if (swapCoroutine != null) StopCoroutine(swapCoroutine);
        ResetPositions();
    }

    private IEnumerator SwapAfterDelay()
    {
        yield return new WaitForSeconds(hoverDelay);

        // Swap Visuals
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
