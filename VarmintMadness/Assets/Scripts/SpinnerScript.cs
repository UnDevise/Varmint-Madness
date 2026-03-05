using UnityEngine;
using System.Collections;

public class ChildPivotRotator : MonoBehaviour
{
    [Header("References")]
    public Transform pivotPoint;         // Drag your child object here

    [Header("Settings")]
    public bool clockwise = true;
    public float secondsBetweenTurns = 2f;
    public float degreesToRotate = 90f;
    public float rotationDuration = 0.5f;

    void Start()
    {
        if (pivotPoint == null)
        {
            Debug.LogError("Please assign a Pivot Point child object!");
            return;
        }
        StartCoroutine(RotationLoop());
    }

    IEnumerator RotationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(secondsBetweenTurns);

            float direction = clockwise ? -1f : 1f;
            float targetTotalRotation = degreesToRotate * direction;

            yield return StartCoroutine(SmoothRotateAround(targetTotalRotation));
        }
    }

    IEnumerator SmoothRotateAround(float totalAngle)
    {
        float elapsed = 0f;
        float lastAngle = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            
            // Calculate how much of the total angle we should have rotated by now
            float currentProgress = elapsed / rotationDuration;
            float currentAngle = Mathf.Lerp(0, totalAngle, currentProgress);
            
            // Rotate only the difference since the last frame to keep it smooth
            float angleDelta = currentAngle - lastAngle;
            transform.RotateAround(pivotPoint.position, Vector3.forward, angleDelta);
            
            lastAngle = currentAngle;
            yield return null;
        }

        // Final adjustment to ensure precision
        float finalDelta = totalAngle - lastAngle;
        transform.RotateAround(pivotPoint.position, Vector3.forward, finalDelta);
    }
}
