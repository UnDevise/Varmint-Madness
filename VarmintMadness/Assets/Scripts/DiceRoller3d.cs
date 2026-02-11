using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DiceRoller : MonoBehaviour
{
    [System.Serializable]
    public struct DiceFace
    {
        public Vector3 localDirection;
        public int value;
    }

    public float rollForce = 5f;
    public float torqueForce = 10f;
    public Vector3 customGravityDirection = new Vector3(0, -9.81f, 0);
    public DiceController diceController;
    public float waitTimeBeforeResult = 1.0f;

    [Header("Result Calculation")]
    public Vector3 targetCalculationPlaneDirection = Vector3.up;

    public DiceFace[] faces;

    private Rigidbody rb;
    private Renderer diceRenderer;
    private bool canRoll = true;
    private bool hasLanded = false;
    private bool thrown = false;

    // NEW — reference to the dice object (D6_Rounded)
    public Transform diceFocusTarget;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Dice object needs a Rigidbody component!");

        diceRenderer = GetComponent<Renderer>();
        if (diceRenderer == null) Debug.LogError("Dice object needs a Renderer component!");

        if (diceController == null)
        {
            diceController = Object.FindAnyObjectByType<DiceController>();
            if (diceController == null) Debug.LogError("DiceController reference missing!");
        }

        if (faces.Length != 6) Debug.LogError("Please configure exactly 6 dice faces in the Inspector!");

        rb.useGravity = false;
        rb.isKinematic = true;
        canRoll = true;

        if (diceRenderer != null) diceRenderer.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canRoll)
        {
            if (diceController == null || !diceController.IsPlayerMoving())
            {
                RollDice();
            }
            else
            {
                Debug.Log("Cannot roll the dice while the player is moving!");
            }
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
        if (diceFocusTarget != null)
        {
            CameraController.Instance.FocusOnDice(diceFocusTarget);
        }

        rb.isKinematic = false;
        canRoll = false;
        thrown = true;
        hasLanded = false;

        if (diceRenderer != null) diceRenderer.enabled = true;

        Vector3 force = new Vector3(Random.Range(0f, rollForce), Random.Range(rollForce / 2f, rollForce), Random.Range(0f, rollForce));
        Vector3 torque = new Vector3(Random.Range(0f, torqueForce), Random.Range(0f, torqueForce), Random.Range(0f, torqueForce));

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);

        StartCoroutine(CheckIfAtRest());
    }

    IEnumerator CheckIfAtRest()
    {
        yield return new WaitUntil(() => rb.linearVelocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f);
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

            if (diceRenderer != null) diceRenderer.enabled = false;

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

