using UnityEngine;

public class MoveTextToScroll : MonoBehaviour
{
    public RectTransform contentArea;
    public RectTransform textToMove;
    public float yOffset = 0f;
    public float contentHeight = 500f; // New variable to set the scrollable height

    void Start()
    {
        if (textToMove != null && contentArea != null)
        {
            // 1. Set the height of the Content area
            // sizeDelta.x stays the same, sizeDelta.y becomes our new height
            contentArea.sizeDelta = new Vector2(contentArea.sizeDelta.x, contentHeight);

            // 2. Parent the text
            textToMove.SetParent(contentArea);

            // 3. Reset position and apply offset
            textToMove.anchoredPosition = new Vector2(0, yOffset);
            textToMove.localScale = Vector3.one;

            textToMove.SetAsLastSibling();
        }
    }
}
