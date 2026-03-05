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
    public float launchForce = 50f;

    private float currentHeight;
    private bool goingUp = false;
    private float timer;

    private float spriteHeight;

    // Track marbles currently on the platform
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

                LaunchMarbles(); // ⭐ NEW — bounce marbles upward
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

    // ⭐ NEW — Apply upward force to marbles
    void LaunchMarbles()
    {
        foreach (Rigidbody rb in marblesOnPlatform)
        {
            if (rb != null)
            {
                // Stop any downward motion
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // Apply a strong upward impulse
                rb.AddForce(Vector3.up * launchForce, ForceMode.Impulse);

                // Optional: small horizontal randomness to prevent straight-up sticking
                Vector3 randomKick = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0f,
                    Random.Range(-0.5f, 0.5f)
                );

                rb.AddForce(randomKick * (launchForce * 0.3f), ForceMode.Impulse);
            }
        }
    }

    // ⭐ Detect marbles on the platform
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !marblesOnPlatform.Contains(rb))
        {
            marblesOnPlatform.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && marblesOnPlatform.Contains(rb))
        {
            marblesOnPlatform.Remove(rb);
        }
    }
    IEnumerator Unstick(Rigidbody rb)
    {
        Collider col = rb.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            yield return new WaitForSeconds(0.05f);
            col.enabled = true;
        }
    }
}