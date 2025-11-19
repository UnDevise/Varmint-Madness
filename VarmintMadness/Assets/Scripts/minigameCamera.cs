using UnityEngine;

public class AutoScrollCamera2D : MonoBehaviour
{
    public float scrollSpeed = 2f;          // Speed at which the camera scrolls down
    public Transform endPoint;              // Assign a GameObject at the bottom of the level
    private bool scrolling = true;

    void Update()
    {
        if (scrolling && endPoint != null)
        {
            // Move camera downward
            transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

            // Stop scrolling when camera reaches end point
            if (transform.position.y <= endPoint.position.y)
            {
                transform.position = new Vector3(transform.position.x, endPoint.position.y, transform.position.z);
                scrolling = false;
            }
        }
    }
}
