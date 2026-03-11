using UnityEngine;
using System;

public class PlayerMinigameMovement : MonoBehaviour
{
    public float moveSpeed = 4f;

    private Vector3 targetPos;
    private bool moving = false;
    private Action onArrive;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void MoveTo(Vector3 pos, Action callback = null)
    {
        targetPos = pos;
        onArrive = callback;
        moving = true;

        if (anim != null)
            anim.SetBool("Running", true);
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

            if (anim != null)
                anim.SetBool("IsWalking", false);

            onArrive?.Invoke();
        }
    }
}