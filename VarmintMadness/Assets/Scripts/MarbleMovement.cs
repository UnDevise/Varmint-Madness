using UnityEngine;

public class MarbleMovement : MonoBehaviour
{
    public float minSpeed = 5f;   // Increase these values for faster marbles
    public float maxSpeed = 12f;
    private Rigidbody2D rb;
    private float randomSpeed;
    private bool raceStarted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Disable gravity if you want pure scripted movement

        // Each marble gets its own random speed
        randomSpeed = Random.Range(minSpeed, maxSpeed);
    }

    public void StartRace()
    {
        raceStarted = true;

        // Give a random horizontal nudge
        float randomX = Random.Range(-2f, 2f);
        rb.AddForce(new Vector2(randomX, 0f), ForceMode2D.Impulse);

        // Gravity will handle the downward movement
        rb.gravityScale = 0.8f; // or higher for faster drop
    }

    void Update()
    {
        if (!raceStarted)
        {
            rb.linearVelocity = Vector2.zero; // Keep marbles still until race starts
        }
    }
}




