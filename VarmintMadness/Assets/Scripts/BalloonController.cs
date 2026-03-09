using UnityEngine;

public class BalloonController : MonoBehaviour
{
    [Header("Balloon Settings")]
    public float minBlowAmount = 0.05f;
    public float maxBlowAmount = 0.15f;

    public float popSize = 2.5f; // balloon pops at this scale
    public float growSpeed = 5f;

    private Vector3 targetScale;

    public bool IsPopped { get; private set; }

    void Start()
    {
        targetScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * growSpeed);
    }

    public bool Blow()
    {
        if (IsPopped) return true;

        float blowAmount = Random.Range(minBlowAmount, maxBlowAmount);
        targetScale += Vector3.one * blowAmount;

        if (targetScale.x >= popSize)
        {
            Pop();
            return true; // popped
        }

        return false; // safe
    }

    void Pop()
    {
        IsPopped = true;
        // TODO: play pop sound, particles, animation
        Destroy(gameObject, 0.1f);
    }
}
