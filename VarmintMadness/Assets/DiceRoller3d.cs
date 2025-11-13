using UnityEngine;

public class DiceRoller : MonoBehaviour
{
    public float rollForce = 5f;
    public float torqueForce = 10f;
    // Keep Use Gravity unchecked in Rigidbody Inspector, we will apply this manually
    public Vector3 customGravityDirection = new Vector3(0, -9.81f, 0);
    private Rigidbody rb;
    private bool canRoll = true; // Prevents rolling while already rolling (optional)

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Dice object needs a Rigidbody component!");
        }

        // Ensure global gravity is off and the dice is kinematic initially
        rb.useGravity = false;
        rb.isKinematic = true;
        canRoll = true;
    }

    void Update()
    {
        // Check for spacebar press
        if (Input.GetKeyDown(KeyCode.Space) && canRoll)
        {
            RollDice();
        }
    }

    void FixedUpdate()
    {
        // Apply custom gravity only when physics is active (not kinematic)
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
