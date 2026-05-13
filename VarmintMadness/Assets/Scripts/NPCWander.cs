using UnityEngine;

public class NPCWander : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float minWalkTime = 1f;
    public float maxWalkTime = 3f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;

    private Vector2 direction;
    private float timer;
    private bool isWalking;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        PickNewState();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
            PickNewState();

        if (isWalking)
        {
            rb.linearVelocity = direction * moveSpeed;
            anim.SetBool("Running", true);

            // ⭐ FLIP BASED ON ACTUAL MOVEMENT VELOCITY
            if (rb.linearVelocity.x > 0.05f)
                sr.flipX = false;   // moving right
            else if (rb.linearVelocity.x < -0.05f)
                sr.flipX = true;    // moving left
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("Running", false);
        }
    }

    void PickNewState()
    {
        isWalking = Random.value > 0.5f;

        if (isWalking)
        {
            direction = Random.insideUnitCircle.normalized;
            timer = Random.Range(minWalkTime, maxWalkTime);
        }
        else
        {
            timer = Random.Range(minIdleTime, maxIdleTime);
        }
    }

    // ⭐ NEW: Flip NPC based on dominant movement direction
    void FlipSpriteBasedOnDirection()
    {
        // Horizontal movement dominates
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            sr.flipX = direction.x < 0;   // left = flipped, right = normal
        }
        else
        {
            // Vertical movement dominates
            // Optional: If you have up/down animations, trigger them here
            // For now, we keep horizontal flip only
        }
    }
}