using UnityEngine;
using System.Collections;

public class SpriteGlider : MonoBehaviour
{
    [Header("Movement Settings")]
    public float distance = 5f;
    public float speed = 2f;
    public float pauseTime = 1f;

    [Header("Direction Settings")]
    [Tooltip("Check this to make it start moving Left first. Uncheck for Right.")]
    public bool startLeft = false;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;
    private bool isPausing = false;

    void Start()
    {
        startPos = transform.position;

        // Calculate target based on the chosen direction
        float direction = startLeft ? -1f : 1f;
        targetPos = startPos + (Vector3.right * distance * direction);
    }

    void Update()
    {
        if (isPausing) return;

        Vector3 destination = movingToTarget ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            StartCoroutine(PauseAndFlip());
        }
    }

    IEnumerator PauseAndFlip()
    {
        isPausing = true;
        yield return new WaitForSeconds(pauseTime);

        movingToTarget = !movingToTarget;
        isPausing = false;
    }
}
