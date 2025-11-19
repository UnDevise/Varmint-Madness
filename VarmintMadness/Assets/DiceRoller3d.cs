using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public float rollForce = 5f;
    public float torqueForce = 10f;
    public Vector3 customGravityDirection = new Vector3(0, -9.81f, 0);
    private Rigidbody rb;
    private bool canRoll = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Dice object needs a Rigidbody component!");
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
        rb.isKinematic = false; // ENABLE physics simulation
        canRoll = false; // Prevent multiple rolls at once

        Vector3 force = new Vector3(Random.Range(0f, rollForce), Random.Range(rollForce / 2f, rollForce), Random.Range(0f, rollForce));
        Vector3 torque = new Vector3(Random.Range(0f, torqueForce), Random.Range(0f, torqueForce), Random.Range(0f, torqueForce));

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);

        // You would typically add logic here to re-enable canRoll = true 
        // once the dice's velocity has reached zero for a short period.
    }
}
