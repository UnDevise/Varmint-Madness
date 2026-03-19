using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IndicatorTarget : MonoBehaviour
{
    public Color arrowColor = Color.white;

    // We now accept the specific sprite and color from the DiceController
    public void InitializeIndicator(Sprite customSprite, Color customColor)
    {
        // Update local variables just in case
        arrowColor = customColor;

        if (OffscreenIndicatorManager.Instance != null)
        {
            // Register with the manager using the EXACT data passed in
            OffscreenIndicatorManager.Instance.AddTarget(transform, customSprite, customColor);
        }
    }

    private void OnDestroy()
    {
        if (OffscreenIndicatorManager.Instance != null)
        {
            OffscreenIndicatorManager.Instance.RemoveTarget(transform);
        }
    }
}
