using System.Collections.Generic;
using UnityEngine;

public class StoreChildPositions : MonoBehaviour
{
    // A list to store the positions of the children.
    // The [SerializeField] attribute makes it visible in the Inspector for debugging.
    [SerializeField]
    private List<Vector2> childPositions = new List<Vector2>();

    private void Start()
    {
        // Call the method to find and store the positions of all children.
        StoreAllChildPositions();

        // (Optional) Log the stored positions to the Console for verification.
        LogChildPositions();
    }

    /// <summary>
    /// Finds all child objects of the parent and stores their world positions.
    /// </summary>
    private void StoreAllChildPositions()
    {
        // Clear the list to avoid adding duplicates if the method is called multiple times.
        childPositions.Clear();

        // Iterate through all the child transforms of this GameObject's transform.
        // `transform` refers to the Transform component of the GameObject the script is attached to.
        foreach (Transform child in transform)
        {
            // For a 2D game, we only need the x and y components.
            // child.position returns the world position, which is usually what you want.
            childPositions.Add(child.position);
        }

        // The alternative is to use a for loop with childCount and GetChild().
        /*
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            childPositions.Add(child.position);
        }
        */
    }

    /// <summary>
    /// Logs all the stored child positions to the Unity Console.
    /// </summary>
    private void LogChildPositions()
    {
        if (childPositions.Count == 0)
        {
            Debug.Log("No child objects were found.");
            return;
        }

        Debug.Log("Found and stored positions of " + childPositions.Count + " children:");
        for (int i = 0; i < childPositions.Count; i++)
        {
            Debug.Log("Child " + i + " position: " + childPositions[i]);
        }
    }
}