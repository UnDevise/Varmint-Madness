using System;
using UnityEngine;
using UnityEngine.Rendering;

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

        Debug.Log("No sides within tolerance");
    }

    internal void RollDice(float _throwForce, float _rollForce, int _diceIndex)
    {
        diceIndex = _diceIndex;

        float randomVarience = Random.Range(-1f, 1f);
        rb.AddForce(transform.forward * (_throwForce + randomVarience), ForceMode.Impulse);

        float rollX = Random.Range(0f, 1f);
        float rollY = Random.Range(0f, 1f);
        float rollZ = Random.Range(0f, 1f);

        rb.AddTorque(new Vector3(rollX, rollY, rollZ) * (_rollForce + randomVarience));

    }
}
