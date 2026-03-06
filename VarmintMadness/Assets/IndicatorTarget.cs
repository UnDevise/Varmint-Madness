using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IndicatorTarget : MonoBehaviour
{
    public Color arrowColor = Color.white;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        OffscreenIndicatorManager.Instance.AddTarget(transform, sr.sprite, arrowColor);
    }

    private void OnDestroy()
    {
        if (OffscreenIndicatorManager.Instance != null)
        {
            OffscreenIndicatorManager.Instance.RemoveTarget(transform);
        }
    }
}