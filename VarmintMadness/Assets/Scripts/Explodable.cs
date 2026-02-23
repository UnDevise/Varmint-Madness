using UnityEngine;

public class Explodable : MonoBehaviour
{
    public GameObject bloodPrefab; // Drag your blood prefab here in the Inspector

    // This triggers when the user clicks the sprite's collider
    void OnMouseDown()
    {
        // 1. Spawn the blood particles at the sprite's position
        Instantiate(bloodPrefab, transform.position, Quaternion.identity);

        // 2. Destroy the original sprite object
        Destroy(gameObject);
    }
}
