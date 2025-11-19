using UnityEngine;

public class MarbleMovement : MonoBehaviour
{
    public float minSpeed = 2f;   // Minimum speed
    public float maxSpeed = 6f;   // Maximum speed
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        
        float randomSpeed = Random.Range(minSpeed, maxSpeed);

        
        rb.linearVelocity = new Vector2(0f, -randomSpeed);
    }
}
