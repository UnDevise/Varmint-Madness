using UnityEngine;

public class LetterBreaker : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        // Get the Rigidbody2D that you added to this letter
        rb = GetComponent<Rigidbody2D>();

        // Start "frozen" so it doesn't fall immediately
        rb.simulated = false;
    }

    // This function automatically runs when the collider is clicked
    void OnMouseDown()
    {
        // 1. "Free" the letter from its parent/layout group
        transform.SetParent(null);

        // 2. Turn on the physics
        rb.simulated = true;

        // 3. Optional: Add a tiny "pop" or rotation
        rb.AddForce(new Vector2(Random.Range(-20f, 20f), 50f));

        // 4. Disappear after 3 seconds
        Destroy(gameObject, 3f);
    }
}
