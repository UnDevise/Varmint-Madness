using UnityEngine;

public class CreditsStopScript : MonoBehaviour
{
    public float scrollSpeed = 40f;
    public float stopYPosition = 500f;
    public float slowDownDistance = 150f; // Distance before the stop where slowing begins

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float currentY = rectTransform.anchoredPosition.y;

        if (currentY < stopYPosition)
        {
            float distanceRemaining = stopYPosition - currentY;

            float currentSpeed = scrollSpeed;

            // Start slowing down when close to the stop point
            if (distanceRemaining < slowDownDistance)
            {
                float slowFactor = distanceRemaining / slowDownDistance;
                currentSpeed *= slowFactor;
            }

            rectTransform.anchoredPosition += new Vector2(0, currentSpeed * Time.deltaTime);

            // Clamp exactly at stop point
            if (rectTransform.anchoredPosition.y > stopYPosition)
            {
                rectTransform.anchoredPosition = new Vector2(
                    rectTransform.anchoredPosition.x,
                    stopYPosition
                );
            }
        }
    }
}