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
    private Animator anim;   // ⭐ NEW

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();   // ⭐ NEW
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
            anim.SetBool("Running", true);   // ⭐ NEW
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            anim.SetBool("Running", false);  // ⭐ NEW
        }
    }

    void PickNewState()
    {
        // 50% chance to walk, 50% to idle
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
}