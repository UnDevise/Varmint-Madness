using UnityEngine;
using UnityEngine.UI;

public class PlayerColorBoxUI : MonoBehaviour
{
    public Image colorImage;          // UI Image whose color we change
    public Transform target;          // Player to follow
    public Vector3 offset = new Vector3(0, -50f, 0);

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
        transform.position = screenPos + offset;
    }

    public void SetColor(Color c)
    {
        if (colorImage != null)
            colorImage.color = c;
    }
}