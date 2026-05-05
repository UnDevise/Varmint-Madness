using UnityEngine;

public class RunnerController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 4f;          // Starting speed
    public float maxSpeed = 12f;          // Maximum speed
    public float acceleration = 2f;       // Speed gained per second
    public float slowdownAmount = 5f;     // Speed lost when hitting obstacle

    [Header("Knockback Settings")]
    public float knockbackForce = 6f;     // How hard the player is pushed back
    public float knockbackDuration = 0.15f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private bool isGrounded = true;
    private float currentSpeed;

    private bool isKnockback = false;
    private float knockbackTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = baseSpeed;
    }

    void Update()
    {
        // If in knockback, skip normal movement
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockback = false;
            }
            return;
        }

        // Accelerate over time
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);

        // Auto-run
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ground detection
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            return;
        }

        // Obstacle hit
        if (collision.collider.CompareTag("Obstacle"))
        {
            // Lose speed
            currentSpeed -= slowdownAmount;
            currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);

            // Enter knockback state
            isKnockback = true;
            knockbackTimer = knockbackDuration;

            // Stop current movement and push back
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(-knockbackForce, 0f), ForceMode2D.Impulse);

            // Camera shake
            Camera.main.GetComponent<CameraFollow2D>().ShakeCamera();
        }
    }
}