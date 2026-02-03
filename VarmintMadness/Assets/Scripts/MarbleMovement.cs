using UnityEngine;

public class MarbleMovement : MonoBehaviour
{
    public float minSpeed = 5f;
    public float maxSpeed = 12f;

    private Rigidbody2D rb;
    private float randomSpeed;
    private bool raceStarted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        randomSpeed = Random.Range(minSpeed, maxSpeed);
    }

    public void StartRace()
    {
        raceStarted = true;

        float randomX = Random.Range(-2f, 2f);
        rb.AddForce(new Vector2(randomX, 0f), ForceMode2D.Impulse);

        rb.gravityScale = 0.8f;
    }

    void Update()
    {
        if (!raceStarted)
        {
            rb.linearVelocity = Vector2.zero; // keep still before race
        }
        // Once race starts, physics handles movement & bouncing
    }
}





