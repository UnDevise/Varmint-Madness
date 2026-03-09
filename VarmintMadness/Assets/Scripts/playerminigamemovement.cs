using UnityEngine;
using System.Collections;

public class PlayerMinigameMovement : MonoBehaviour
{
    public float moveSpeed = 4f;

    private Vector3 targetPos;
    private bool moving = false;
    private System.Action onArrive;

    public void MoveTo(Vector3 pos, System.Action callback = null)
    {
        targetPos = pos;
        onArrive = callback;
        moving = true;
    }

    void Update()
    {
        if (!moving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            moving = false;
            onArrive?.Invoke();
        }
    }
}
