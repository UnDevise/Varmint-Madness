using UnityEngine;
using UnityEngine.UI;

public class OffscreenIndicator : MonoBehaviour
{
    public Image arrowImage;
    public Image targetIcon;

    [Header("Rotation Settings")]
    public float rotationOffset = 0f;

    [Header("Screen Edge Settings")]
    public float edgeDistance = 80f; // distance from edge of screen

    [Header("Distance Scaling")]
    public float maxIndicatorDistance = 100f; // beyond this the arrow disappears
    public float minScale = 0.4f;
    public float maxScale = 1.2f;

    private Transform target;
    private Camera cam;
    private RectTransform rectTransform;

    public void Initialize(Transform newTarget, Sprite sprite, Color arrowColor, Camera cameraRef)
    {
        target = newTarget;
        cam = cameraRef;

        rectTransform = GetComponent<RectTransform>();

        targetIcon.sprite = sprite;
        arrowImage.color = arrowColor;
    }

    public void UpdateIndicator(bool show)
    {
        if (!show || target == null)
        {
            arrowImage.enabled = false;
            targetIcon.enabled = false;
            return;
        }

        float worldDistance = Vector2.Distance(target.position, cam.transform.position);

        // Hide indicator if target is too far away
        if (worldDistance > maxIndicatorDistance)
        {
            arrowImage.enabled = false;
            targetIcon.enabled = false;
            return;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);

        bool isOffscreen =
            screenPos.x <= 0 ||
            screenPos.x >= Screen.width ||
            screenPos.y <= 0 ||
            screenPos.y >= Screen.height ||
            screenPos.z < 0;

        if (!isOffscreen)
        {
            arrowImage.enabled = false;
            targetIcon.enabled = false;
            return;
        }

        arrowImage.enabled = true;
        targetIcon.enabled = true;

        Vector3 center = new Vector3(Screen.width / 2f, Screen.height / 2f);
        Vector3 dir = (screenPos - center).normalized;

        if (screenPos.z < 0)
            dir *= -1;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        float x = Mathf.Clamp(center.x + dir.x * center.x, edgeDistance, Screen.width - edgeDistance);
        float y = Mathf.Clamp(center.y + dir.y * center.y, edgeDistance, Screen.height - edgeDistance);

        rectTransform.position = new Vector3(x, y, 0);

        // Distance-based scaling
        float distancePercent = worldDistance / maxIndicatorDistance;
        float scale = Mathf.Lerp(maxScale, minScale, distancePercent);

        rectTransform.localScale = new Vector3(scale, scale, 1f);
    }
}