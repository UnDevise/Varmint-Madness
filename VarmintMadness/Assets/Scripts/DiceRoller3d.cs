using UnityEngine;
using System.Collections;

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

    private Camera mainCamera;
    public float diceSpawnOffsetFromCamera = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        diceRenderer = GetComponent<Renderer>();

        if (diceController == null)
            diceController = FindAnyObjectByType<DiceController>();

        rb.useGravity = false;
        rb.isKinematic = true;

        if (diceRenderer != null)
            diceRenderer.enabled = false;

        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canRoll)
        {
            if (diceController == null || !diceController.IsPlayerMoving())
            {
                RollDice();
            }
        }

        // ⭐ Dice no longer moves the camera at all
    }

    void FixedUpdate()
    {
        if (!rb.isKinematic)
            rb.AddForce(customGravityDirection, ForceMode.Acceleration);
    }

    void RollDice()
    {
        // Tell camera to focus on dice
        if (CameraController.Instance != null)
            CameraController.Instance.FocusDice(transform);

        rb.isKinematic = false;
        canRoll = false;
        thrown = true;
        hasLanded = false;

        if (diceRenderer != null)
            diceRenderer.enabled = true;

        Vector3 force = new Vector3(
            Random.Range(0f, rollForce),
            Random.Range(rollForce / 2f, rollForce),
            Random.Range(0f, rollForce)
        );

        Vector3 torque = new Vector3(
            Random.Range(0f, torqueForce),
            Random.Range(0f, torqueForce),
            Random.Range(0f, torqueForce)
        );

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);

        StartCoroutine(CheckIfAtRest());
    }


    IEnumerator CheckIfAtRest()
    {
        yield return new WaitUntil(() =>
            rb.linearVelocity.sqrMagnitude < 0.01f &&
            rb.angularVelocity.sqrMagnitude < 0.01f
        );

        yield return new WaitForSeconds(waitTimeBeforeResult);

        if (!hasLanded && thrown)
        {
            hasLanded = true;
            rb.useGravity = false;
            rb.isKinematic = true;

            int rollResult = CalculateDiceResult();
            Debug.Log("Dice landed on: " + rollResult);

            // Move the player
            if (diceController != null)
                diceController.MoveCurrentPlayer(rollResult);

            // Switch camera to the player
            if (CameraController.Instance != null && diceController != null)
            {
                PlayerMovement current = diceController.playersToMove[diceController.currentPlayerIndex];
                CameraController.Instance.FollowPlayer(current.transform);
            }

            if (diceRenderer != null)
                diceRenderer.enabled = false;

            thrown = false;
            canRoll = true;
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