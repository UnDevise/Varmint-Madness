using UnityEngine;
using System.Collections;

public class UISlidePanel : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform panel;

    [Header("Slide Settings")]
    public float slideDistance = 300f;
    public float slideSpeed = 6f;
    public bool slideRight = true; // true = right, false = left

    private bool isOpen = false;
    private Vector2 closedPos;
    private Vector2 openPos;

    private Coroutine slideRoutine;

    void Start()
    {
        closedPos = panel.anchoredPosition;

        float direction = slideRight ? 1f : -1f;
        openPos = closedPos + new Vector2(direction * slideDistance, 0);
    }

    public void TogglePanel()
    {
        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        isOpen = !isOpen;

        slideRoutine = StartCoroutine(Slide());
    }

    IEnumerator Slide()
    {
        Vector2 target = isOpen ? openPos : closedPos;

        while (Vector2.Distance(panel.anchoredPosition, target) > 0.1f)
        {
            panel.anchoredPosition = Vector2.Lerp(
                panel.anchoredPosition,
                target,
                Time.deltaTime * slideSpeed
            );

            yield return null;
        }

        panel.anchoredPosition = target;
    }
}