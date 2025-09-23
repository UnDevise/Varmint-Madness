using UnityEngine;

public class AlienTitleMovement : MonoBehaviour
{
    public Transform PointA;
    public Transform PointB;
    public float moveSpeed = 2f;

    private Vector3 nextPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      nextPosition = PointB.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

        if (transform.position == nextPosition)
        {
            nextPosition = (nextPosition == PointA.position) ? PointB.position : PointA.position;
        }

        if (transform.position == PointA.position)
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1; // Invert the X-axis
            transform.localScale = scale;
        }

        if (transform.position == PointB.position)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x); // Ensure X scale is positive
            transform.localScale = currentScale;
        }
    }
}
