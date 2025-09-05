using UnityEngine;

public class DestroyOnTouch : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Add a debug message to confirm the trigger is being detected.
        Debug.Log("Trigger detected with " + other.gameObject.name);

        // Check that we are not destroying our own square.
        if (other.gameObject != this.gameObject)
        {
            // Destroy the other GameObject.
            Destroy(other.gameObject);
        }
    }
}