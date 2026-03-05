using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    public float launchForce = 25f;   // Increase for stronger launch

    private float currentHeight;
    private bool goingUp = false;
    private float timer;

    private float spriteHeight;

    private List<Rigidbody> marblesOnPlatform = new List<Rigidbody>();

    void Start()
    {
        SpriteRenderer sr = midSection.GetComponent<SpriteRenderer>();
        spriteHeight = sr.bounds.size.y;

        currentHeight = maxHeight;
        UpdateLift();
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
        foreach (Rigidbody rb in marblesOnPlatform)
        {
            if (rb != null)
                StartCoroutine(LaunchRoutine(rb));
        }
    }

    IEnumerator LaunchRoutine(Rigidbody rb)
    {
        Collider col = rb.GetComponent<Collider>();

        // Disable collider so it doesn't stick to the platform
        if (col != null)
            col.enabled = false;

        // Reset downward velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Apply strong upward impulse
        rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);

        // Optional: small sideways randomness
        rb.AddForce(new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ) * (launchForce * 0.2f), ForceMode.Impulse);

        // Wait briefly before re-enabling collider
        yield return new WaitForSeconds(0.05f);

        if (col != null)
            col.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !marblesOnPlatform.Contains(rb))
            marblesOnPlatform.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
            marblesOnPlatform.Remove(rb);
    }
}