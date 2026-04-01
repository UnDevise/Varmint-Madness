using UnityEngine;

public class CreditsScript : MonoBehaviour
{
    public float scrollSpeed = 40f;

    private RectTransform parentRectTransform;

    void Start()
    {
        // This gets the RectTransform of the parent object
        if (transform.parent != null)
        {
            parentRectTransform = transform.parent.GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        // Moves the parent if it exists
        if (parentRectTransform != null)
        {
            parentRectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
        }
    }
}
