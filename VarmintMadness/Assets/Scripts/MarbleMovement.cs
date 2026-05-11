using UnityEngine;

public class MarbleMovement : MonoBehaviour
{
    public int marbleIndex;
    public float minSpeed = 5f;
    public float maxSpeed = 12f;
    private Rigidbody2D rb;
    private float randomSpeed;
    private bool raceStarted = false;

    public float stuckVelocityThreshold = 0.05f;
    public float stuckTimeNeeded = 1.0f;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;
    public bool IsStuck { get; private set; } = false;
    public bool isGhostMarble = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        randomSpeed = Random.Range(minSpeed, maxSpeed);
        lastPosition = transform.position;
    }

    public void StartRace()
    {
        raceStarted = true;
        float randomX = Random.Range(-2f, 2f);
        rb.AddForce(new Vector2(randomX, 0f), ForceMode2D.Impulse);
        float randomTorque = Random.Range(-5f, 5f);
        rb.AddTorque(randomTorque, ForceMode2D.Impulse);

        // Ghost marble gets boosted gravity to fall straight through everything
        rb.gravityScale = isGhostMarble ? 5f : 0.8f;
    }

    void Update()
    {
        if (!raceStarted)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            return;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }

        CheckIfStuck();
    }

    public void CheckIfStuck()
    {
        if (rb.linearVelocity.magnitude > 0.2f)
        {
            stuckTimer = 0f;
            IsStuck = false;
            lastPosition = transform.position;
            return;
        }

        if (rb.IsTouchingLayers() == false)
        {
            stuckTimer = 0f;
            IsStuck = false;
            lastPosition = transform.position;
            return;
        }

        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < 0.01f)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckTimeNeeded)
                IsStuck = true;
        }
        else
        {
            stuckTimer = 0f;
            IsStuck = false;
        }

        lastPosition = transform.position;
    }

    public void PushFree()
    {
        if (rb == null) return;

        Vector2 randomPush = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1.5f)
        ).normalized * 3f;

        rb.AddForce(randomPush, ForceMode2D.Impulse);
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