using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// PlayerController — Attach to each of the 4 player GameObjects.
/// Handles movement, walking animation, and the per-player HUD (name + points).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    //  INSPECTOR SETTINGS
    // ─────────────────────────────────────────────────────────────────────────

    [Header("── Player Identity ──")]
    public string playerName = "Player 1";

    [Header("── HUD References ──")]
    [Tooltip("TextMeshPro label that shows the player name (world-space canvas).")]
    public TextMeshProUGUI nameLabel;

    [Tooltip("TextMeshPro label that shows the current point total (world-space canvas).")]
    public TextMeshProUGUI pointsLabel;

    [Header("── Animator Parameters ──")]
    [Tooltip("Name of the Bool parameter in the Animator that triggers the walk animation.")]
    public string walkBoolParameter = "IsWalking";

    [Tooltip("Name of the Float parameter for horizontal direction (used to flip sprite via Animator).")]
    public string horizontalParameter = "Horizontal";

    [Header("── Movement ──")]
    [Tooltip("How close (in units) the player must be to the waypoint to count as 'arrived'.")]
    [Range(0.01f, 0.5f)]
    public float arrivalThreshold = 0.1f;

    // ─────────────────────────────────────────────────────────────────────────
    //  PUBLIC READ-ONLY PROPERTIES
    // ─────────────────────────────────────────────────────────────────────────

    public int Points { get; private set; } = 0;

    // ─────────────────────────────────────────────────────────────────────────
    //  PRIVATE FIELDS
    // ─────────────────────────────────────────────────────────────────────────

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 targetPosition;
    private float walkSpeed;
    private bool isWalking = false;
    private bool reachedDestination = true;

    // ─────────────────────────────────────────────────────────────────────────
    //  UNITY LIFECYCLE
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Freeze rotation so the character doesn't tumble
        rb.freezeRotation = true;
        rb.gravityScale = 0f; // top-down / side-scrolling show — no gravity needed
    }

    private void Start()
    {
        RefreshHUD();
    }

    private void FixedUpdate()
    {
        if (!isWalking) return;

        Vector2 currentPos = rb.position;
        Vector2 direction = (targetPosition - currentPos);
        float distance = direction.magnitude;

        if (distance <= arrivalThreshold)
        {
            // Snap to target and stop
            rb.MovePosition(targetPosition);
            isWalking = false;
            reachedDestination = true;
            SetWalkingAnimation(false, 0f);
            return;
        }

        // Move towards target
        Vector2 moveDir = direction.normalized;
        rb.MovePosition(currentPos + moveDir * walkSpeed * Time.fixedDeltaTime);

        // Pass horizontal direction to Animator (for sprite flipping or directional blending)
        SetWalkingAnimation(true, moveDir.x);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  PUBLIC API  (called by GameShowManager)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Begins walking the player toward the given world position.</summary>
    public void StartWalking(Vector2 destination, float speed)
    {
        targetPosition = destination;
        walkSpeed = speed;
        isWalking = true;
        reachedDestination = false;
    }

    /// <summary>Immediately halts movement and idles the animation.</summary>
    public void StopWalking()
    {
        isWalking = false;
        reachedDestination = true;
        rb.linearVelocity = Vector2.zero;
        SetWalkingAnimation(false, 0f);
    }

    /// <summary>Returns true once the player has arrived at the waypoint.</summary>
    public bool HasReachedDestination() => reachedDestination;

    /// <summary>Adds points and refreshes the HUD immediately.</summary>
    public void AddPoints(int amount)
    {
        Points += amount;
        RefreshHUD();
        StartCoroutine(AnimatePointsBump());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  ANIMATION
    // ─────────────────────────────────────────────────────────────────────────

    private void SetWalkingAnimation(bool walking, float horizontal)
    {
        if (animator == null) return;

        animator.SetBool(walkBoolParameter, walking);
        animator.SetFloat(horizontalParameter, horizontal);

        // Mirror sprite based on movement direction (alternative to Animator flipping)
        if (spriteRenderer != null && Mathf.Abs(horizontal) > 0.01f)
            spriteRenderer.flipX = horizontal < 0f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HUD
    // ─────────────────────────────────────────────────────────────────────────

    private void RefreshHUD()
    {
        if (nameLabel != null)
            nameLabel.text = playerName;

        if (pointsLabel != null)
            pointsLabel.text = $"{Points} pts";
    }

    /// <summary>Quick scale pulse on the points label when points are awarded.</summary>
    private IEnumerator AnimatePointsBump()
    {
        if (pointsLabel == null) yield break;

        Vector3 originalScale = pointsLabel.transform.localScale;
        Vector3 bigScale = originalScale * 1.5f;

        float elapsed = 0f;
        float half = 0.15f;

        while (elapsed < half)
        {
            pointsLabel.transform.localScale = Vector3.Lerp(originalScale, bigScale, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            pointsLabel.transform.localScale = Vector3.Lerp(bigScale, originalScale, elapsed / half);
            elapsed += Time.deltaTime;
            yield return null;
        }

        pointsLabel.transform.localScale = originalScale;
    }
}
