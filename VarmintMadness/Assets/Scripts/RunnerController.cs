using UnityEngine;

public class RunnerController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float baseSpeed = 4f;
    public float maxSpeed = 12f;
    public float acceleration = 2f;
    public float slowdownAmount = 5f;

    [Header("Knockback Settings")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.15f;

    [Header("Jump Settings")]
    public float jumpForce = 10f;

    [HideInInspector] public bool canFinish = false;

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
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
                isKnockback = false;

            return;
        }

        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            return;
        }

        if (collision.collider.CompareTag("Obstacle"))
        {
            currentSpeed -= slowdownAmount;
            currentSpeed = Mathf.Clamp(currentSpeed, baseSpeed, maxSpeed);

            isKnockback = true;
            knockbackTimer = knockbackDuration;

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(-knockbackForce, 0f), ForceMode2D.Impulse);

            Camera.main.GetComponent<CameraFollow2D>().ShakeCamera();
        }
    }

    public void ResetRunner(Vector3 startPos)
    {
        transform.position = startPos;
        rb.linearVelocity = Vector2.zero;
        currentSpeed = baseSpeed;
        isKnockback = false;
        canFinish = false;
    }
}