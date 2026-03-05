using UnityEngine;

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
    public float platformOffset = 0f; // lets you fine tune platform position

    private float currentHeight;
    private bool goingUp = false;
    private float timer;

    private float spriteHeight;

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
        pos.y = height + platformOffset; // adjustable alignment
        platform.localPosition = pos;
    }
}