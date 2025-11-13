using System;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [SerializeField] private float tolerance = 0.99f;

    private Rigidbody rb;
    private bool hasStoppedRolling = false;
    private bool hasThrowDelayFinished = false;
    private int diceIndex = -1;

    public static event Action<int, int> OnDiceResult;

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
            transform.forward.normalized, //1 Blue
            transform.right.normalized, //2 Red
            transform.up.normalized, //5 Green
            -transform.forward.normalized, //3
            -transform.right.normalized, //4
            -transform.up.normalized //6
        };

        for (int i = 0; i < sides.Length; i++)
        {
            if (Vector3.Dot(sides[i], Vector3.up)   > tolerance)
            {
                OnDiceResult?.Invoke(diceIndex, i+1);
            }
        }
    }
}
