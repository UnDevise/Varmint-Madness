using UnityEngine;

public class MarbleMovement : MonoBehaviour
{
    public int marbleIndex;          // Set in Inspector (0,1,2,...)
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

        float randomTorque = Random.Range(-5f, 5f);
        rb.AddTorque(randomTorque, ForceMode2D.Impulse);

        rb.gravityScale = 0.8f;
    }

    void Update()
    {
        if (!raceStarted)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Marble"))
        {
            Vector2 pushDir = (transform.position - collision.transform.position).normalized;
            rb.AddForce(pushDir * 2f, ForceMode2D.Impulse);
        }
    }
}





