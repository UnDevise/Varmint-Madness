using UnityEngine;

public class BalloonController : MonoBehaviour
{
    [Header("Balloon Settings")]
    public float minBlowAmount = 0.05f;
    public float maxBlowAmount = 0.15f;
    public float popSize = 2.5f;
    public float growSpeed = 5f;

    [Header("Position Offset")]
    public Vector3 offsetDirection = new Vector3(0, 1, 0); // Direction balloon shifts (default = up)
    public float offsetMultiplier = 1f; // How much it moves as it grows

    [Header("Pump Rules")]
    public int maxPumpsPerTurn = 3;

    [Header("Points")]
    public int pointsPerPump = 10;

    private int pumpsThisTurn = 0;
    private Vector3 targetScale;

    private Vector3 startPosition;
    private Vector3 startScale;

    public bool IsPopped { get; private set; }
    public int[] playerPoints;

    void Start()
    {
        targetScale = transform.localScale;

        startScale = transform.localScale;
        startPosition = transform.position;
    }

    void Update()
    {
        // Smooth scale
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * growSpeed
        );

        // Calculate how much we've grown relative to start
        float growthAmount = transform.localScale.x - startScale.x;

        // Apply offset based on growth
        Vector3 offset = offsetDirection.normalized * growthAmount * offsetMultiplier;
        transform.position = startPosition + offset;
    }

    public void InitializePlayers(int playerCount)
    {
        playerPoints = new int[playerCount];
    }

    public bool Pump(int playerIndex)
    {
        if (IsPopped) return true;
        if (pumpsThisTurn >= maxPumpsPerTurn) return false;

        pumpsThisTurn++;

        float blowAmount = Random.Range(minBlowAmount, maxBlowAmount);
        targetScale += Vector3.one * blowAmount;

        playerPoints[playerIndex] += pointsPerPump;

        if (targetScale.x >= popSize)
        {
            Pop(playerIndex);
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

    void Pop(int playerIndex)
    {
        IsPopped = true;
        playerPoints[playerIndex] = 0;
        Destroy(gameObject, 0.1f);
    }
}