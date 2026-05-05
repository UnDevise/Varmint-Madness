using UnityEngine;

public class RunnerController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public Rigidbody2D rb;

    private bool isGrounded = true;

    void Update()
    {
        rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
            isGrounded = true;
    }
}