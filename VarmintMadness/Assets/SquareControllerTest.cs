using UnityEngine;

public class SimpleWASDMovement : MonoBehaviour
{
    // The movement speed of the square, adjustable in the Unity Inspector.
    [SerializeField] private float speed = 5f;

    // The Update method is called once per frame.
    void Update()
    {
        // Get input from the "Horizontal" and "Vertical" axes.
        // These are pre-configured to respond to WASD and arrow keys.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Create a new Vector2 to represent the movement direction.
        // The .normalized property ensures that moving diagonally is not faster.
        Vector2 movement = new Vector2(horizontalInput, verticalInput).normalized;

        // Use transform.Translate to move the object directly.
        // Time.deltaTime ensures the movement is smooth and frame-rate independent.
        transform.Translate(movement * speed * Time.deltaTime);
    }
}