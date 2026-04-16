using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;
using System.Linq;

public class ScissorLift : MonoBehaviour
{
    [Header("References")]
    public Transform midSection;
    public Transform platform;

    [Header("Height Settings")]
    public float minHeight = 1f;
    public float maxHeight = 5f;

    [Header("Movement Speeds")]
    public float descendSpeed = 1f;
    public float ascendSpeed = 6f;

    [Header("Timing")]
    public float waitAtBottom = 1f;
    public float waitAtTop = 1f;

    [Header("Platform Alignment")]
    public float platformOffset = 0f;

    [Header("Bounce Settings")]
    public float launchForce = 60f;

    private float currentHeight;
    private bool goingUp = false;
    private float timer;
    public Collider2D platformCollider2D;


    private float spriteHeight;

    private List<Rigidbody2D> marblesOnPlatform = new List<Rigidbody2D>();

    void Start()
    {
        SpriteRenderer sr = midSection.GetComponent<SpriteRenderer>();
        spriteHeight = sr.bounds.size.y;

        currentHeight = maxHeight;
        UpdateLift();
    }
    private void Awake()
    {
        FindPlatformCollider();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0)
            return;

        if (goingUp)
        {
            currentHeight += ascendSpeed * Time.deltaTime;

            if (currentHeight >= maxHeight)
            {
                currentHeight = maxHeight;
                goingUp = false;
                timer = waitAtTop;

                LaunchMarbles(); // Launch at the top
            }
        }
        else
        {
            currentHeight -= descendSpeed * Time.deltaTime;

            if (currentHeight <= minHeight)
            {
                currentHeight = minHeight;
                goingUp = true;
                timer = waitAtBottom;
            }
        }

        UpdateLift();
    }

    void UpdateLift()
    {
        Vector3 scale = midSection.localScale;
        scale.y = currentHeight;
        midSection.localScale = scale;

        float height = spriteHeight * currentHeight;

        Vector3 pos = platform.localPosition;
        pos.y = height + platformOffset;
        platform.localPosition = pos;
    }

    void LaunchMarbles()
    {
        if (platformCollider2D != null)
            platformCollider2D.enabled = false;

        foreach (Rigidbody2D rb in marblesOnPlatform)
            StartCoroutine(LaunchRoutine(rb));

        StartCoroutine(ReenablePlatformCollider());
    }

    IEnumerator ReenablePlatformCollider()
    {
        yield return new WaitForSeconds(0.2f);

        if (platformCollider2D != null)
            platformCollider2D.enabled = true;
    }


    IEnumerator LaunchRoutine(Rigidbody2D rb)
    {
        // Disable ALL marble colliders
        foreach (var c in rb.GetComponents<Collider2D>())
            c.enabled = false;

        // Move marble up slightly so it's not touching anything
        rb.position += Vector2.up * 0.2f;
        yield return null; // wait 1 frame

        // Reset velocity
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Launch
        rb.AddForce(Vector2.up * launchForce, ForceMode2D.Impulse);

        // Re-enable colliders after short delay
        yield return new WaitForSeconds(0.15f);

        foreach (var c in rb.GetComponents<Collider2D>())
            c.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null && !marblesOnPlatform.Contains(rb))
            marblesOnPlatform.Add(rb);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
            marblesOnPlatform.Remove(rb);
    }

    private void FindPlatformCollider()
    {
        Transform holder = transform.Find("LiftHolderScaled");

        if (holder == null)
        {
            Debug.LogError("ScissorLift: Could not find LiftHolderScaled in children!");
            return;
        }

        Collider2D[] cols = holder.GetComponents<Collider2D>();

        if (cols.Length == 0)
        {
            Debug.LogError("ScissorLift: LiftHolderScaled has no 2D colliders!");
            return;
        }

        foreach (Collider2D c in cols)
        {
            if (!c.isTrigger)
            {
                platformCollider2D = c;
                Debug.Log("ScissorLift: Found platform solid collider: " + c.name);
                return;
            }
        }

        Debug.LogError("ScissorLift: No solid 2D collider found on LiftHolderScaled!");
    }
}