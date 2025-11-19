using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiceRoller : MonoBehaviour
{
    // Struct to define face directions and values in the Inspector
    [System.Serializable]
    public struct DiceFace
    {
        public Vector3 localDirection; // e.g., Vector3.up, Vector3.forward
        public int value;              // The number on that face
    }

    public float rollForce = 5f;
    public float torqueForce = 10f;
    public Vector3 customGravityDirection = new Vector3(0, -9.81f, 0);
    public DiceController diceController;
    public float waitTimeBeforeResult = 1.0f; // Time to wait after stopping

    // Assign your 6 faces here in the Inspector
    public DiceFace[] faces;

    private Rigidbody rb;
    private bool canRoll = true;
    private bool hasLanded = false;
    private bool thrown = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Dice object needs a Rigidbody component!");
        }

        if (diceController == null)
        {
            diceController = FindObjectOfType<DiceController>();
            if (diceController == null)
            {
                Debug.LogError("DiceController reference missing!");
            }
        }

        if (faces.Length != 6)
        {
            Debug.LogError("Please configure exactly 6 dice faces in the Inspector!");
        }

        rb.useGravity = false;
        rb.isKinematic = true;
        canRoll = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canRoll)
        {
            RollDice();
        }
    }

    void FixedUpdate()
    {
        if (!rb.isKinematic)
        {
            rb.AddForce(customGravityDirection, ForceMode.Acceleration);
        }
    }

    void RollDice()
    {
        rb.isKinematic = false;
        canRoll = false;
        thrown = true;
        hasLanded = false;

        Vector3 force = new Vector3(Random.Range(0f, rollForce), Random.Range(rollForce / 2f, rollForce), Random.Range(0f, rollForce));
        Vector3 torque = new Vector3(Random.Range(0f, torqueForce), Random.Range(0f, torqueForce), Random.Range(0f, torqueForce));

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);

        StartCoroutine(CheckIfAtRest());
    }

    IEnumerator CheckIfAtRest()
    {
        // Wait until velocity is near zero
        yield return new WaitUntil(() => rb.linearVelocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f);
        // Wait for the additional specified time to ensure it's truly settled
        yield return new WaitForSeconds(waitTimeBeforeResult);

        if (!hasLanded && thrown)
        {
            hasLanded = true;
            rb.useGravity = false;
            rb.isKinematic = true;
            int rollResult = CalculateDiceResult();
            Debug.Log("Dice landed on: " + rollResult);

            diceController.MoveCurrentPlayer(rollResult);

            canRoll = true;
            thrown = false;
        }
    }

    int CalculateDiceResult()
    {
        int result = 0;
        float maxDot = -Mathf.Infinity;

        foreach (var face in faces)
        {
            // Transform the local direction of the face to world space
            Vector3 worldSpaceDir = transform.TransformDirection(face.localDirection);
            // Calculate dot product with world up direction
            float dot = Vector3.Dot(worldSpaceDir, Vector3.up);

            // The side with the highest dot product is facing upward
            if (dot > maxDot)
            {
                maxDot = dot;
                result = face.value;
            }
        }
        return result;
    }
}
