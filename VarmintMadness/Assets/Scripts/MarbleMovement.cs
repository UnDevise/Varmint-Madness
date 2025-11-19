using UnityEngine;

public class MarbleMovement : MonoBehaviour
{
    public float minSpeed = 2f;   
    public float maxSpeed = 6f;   
    public Vector2 spawnAreaMin;  
    public Vector2 spawnAreaMax; 

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        transform.position = new Vector2(randomX, randomY);

        
        float randomSpeed = Random.Range(minSpeed, maxSpeed);

        
        rb.linearVelocity = new Vector2(0f, -randomSpeed);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Marble"))
        {
            
            Vector2 bounceDir = (transform.position - collision.transform.position).normalized;
            rb.AddForce(bounceDir * 2f, ForceMode2D.Impulse);
        }
    }
}

