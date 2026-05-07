using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 5f;
    public float xOffset = 2f;

    private float fixedY;

    [Header("Shake Settings")]
    public Transform shakeParent;          // NEW: assign the parent object
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.2f;

    private float shakeTimer = 0f;
    private Vector3 parentOriginalPos;

    void Start()
    {
        fixedY = transform.position.y;

        if (shakeParent != null)
            parentOriginalPos = shakeParent.localPosition;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Normal follow movement
            Vector3 desiredPosition = new Vector3(
                target.position.x + xOffset,
                fixedY,
                transform.position.z
            );

            Vector3 smoothedPosition = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );

            transform.position = smoothedPosition;
        }

        // Apply shake to the parent, not the camera
        if (shakeParent != null)
        {
            if (shakeTimer > 0)
            {
                Vector3 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
                shakeParent.localPosition = parentOriginalPos + shakeOffset;

                shakeTimer -= Time.deltaTime;
            }
            else
            {
                shakeParent.localPosition = parentOriginalPos;
            }
        }
    }

    public void ShakeCamera()
    {
        shakeTimer = shakeDuration;
    }
}