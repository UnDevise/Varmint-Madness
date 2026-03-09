using UnityEngine;

public class BalloonController : MonoBehaviour
{
    [Header("Balloon Settings")]
    public float minBlowAmount = 0.05f;
    public float maxBlowAmount = 0.15f;

    public float popSize = 2.5f;
    public float growSpeed = 5f;

    [Header("Pump Rules")]
    public int maxPumpsPerTurn = 3;

    private int pumpsThisTurn = 0;
    private Vector3 targetScale;

    public bool IsPopped { get; private set; }

    void Start()
    {
        targetScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * growSpeed
        );
    }

    // Called when player presses SPACE
    // Returns true if balloon popped
    public bool Pump()
    {
        if (IsPopped) return true;
        if (pumpsThisTurn >= maxPumpsPerTurn) return false;

        pumpsThisTurn++;

        float blowAmount = Random.Range(minBlowAmount, maxBlowAmount);
        targetScale += Vector3.one * blowAmount;

        if (targetScale.x >= popSize)
        {
            Pop();
            return true;
        }

        return false;
    }

    public bool HasPumpsLeft()
    {
        return pumpsThisTurn < maxPumpsPerTurn;
    }

    public bool CanSkip()
    {
        return pumpsThisTurn > 0;
    }

    public void ResetTurn()
    {
        pumpsThisTurn = 0;
    }

    void Pop()
    {
        IsPopped = true;
        // TODO: pop sound, particles, animation
        Destroy(gameObject, 0.1f);
    }
}