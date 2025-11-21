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

    [Header("Result Calculation")]
    // New variable to define the target direction the winning face should point towards
    public Vector3 targetCalculationPlaneDirection = Vector3.up;

    // Assign your 6 faces here in the Inspector
    public DiceFace[] faces;

    private Rigidbody rb;
    private Renderer diceRenderer; // Reference to the Renderer component
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

        diceRenderer = GetComponent<Renderer>(); // Get the Renderer component
        if (diceRenderer == null)
        {
            Debug.LogError("Dice object needs a Renderer component!");
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

        // Make the dice invisible when the scene starts
        if (diceRenderer != null)
        {
            diceRenderer.enabled = false;
        }
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

        // Make the dice visible when rolling
        if (diceRenderer != null)
        {
            diceRenderer.enabled = true;
        }

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

            if (diceController != null)
            {
                diceController.MoveCurrentPlayer(rollResult);
            }

            // Make the dice invisible again after it stops
            if (diceRenderer != null)
            {
                diceRenderer.enabled = false;
            }

            canRoll = true;
            thrown = false;
        }
    }

    int CalculateDiceResult()
    {
        int result = 0;
        float maxDot = -Mathf.Infinity;
        Vector3 normalizedTargetDirection = targetCalculationPlaneDirection.normalized;

        foreach (var face in faces)
        {
            Vector3 worldSpaceDir = transform.TransformDirection(face.localDirection);
            float dot = Vector3.Dot(worldSpaceDir, normalizedTargetDirection);

            if (dot > maxDot)
            {
                maxDot = dot;
                result = face.value;
            }
        }
        return result;
    }
}
