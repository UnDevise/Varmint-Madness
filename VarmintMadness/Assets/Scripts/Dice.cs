using System;
using UnityEngine;

public class Dice : MonoBehaviour
{
    private Rigidbody rb;
    private bool hasStoppedRolling = false;
    private bool hasThrowDelayFinished = false;


    private void Awake()
    {
       rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!hasThrowDelayFinished) { return; }

        if (!hasStoppedRolling && rb.linearVelocity.sqrMagnitude == 0f)
        {
            hasStoppedRolling = true;
            GetSideUp();
        }
    }

    private void GetSideUp()
    {
        Vector3[] sides = new Vector3[]
        {
            transform.forward.normalized,
            transform.right.normalized,
            transform.up.normalized,
            -transform.forward.normalized,
            -transform.right.normalized,
            -transform.up.normalized
        };
    }
}
