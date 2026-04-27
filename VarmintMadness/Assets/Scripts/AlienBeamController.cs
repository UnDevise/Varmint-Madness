using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(1000)]
public class WeightedPlaceholderSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class ControllerEntry
    {
        public string name;
        public RuntimeAnimatorController controller;
        [Range(0, 100)] public float weight;
    }

    [Header("Targeting")]
    public Animator placeholderAnimator;

    [Header("Transform Settings")]
    public Vector3 placeholderScale = Vector3.one;

    [Header("Visual Settings")]
    [Range(0f, 1f)] public float alpha = 1f; // Slider for transparency

    public List<ControllerEntry> controllerList = new List<ControllerEntry>();

    private SpriteRenderer placeholderRenderer;

    void Start()
    {
        if (placeholderAnimator == null)
        {
            Debug.LogError("Drag the Placeholder child into the Placeholder Animator slot!");
            return;
        }

        // Get the SpriteRenderer from the placeholder to control transparency
        placeholderRenderer = placeholderAnimator.GetComponent<SpriteRenderer>();

        PickNextController();
    }

    void Update()
    {
        if (placeholderAnimator == null || placeholderAnimator.runtimeAnimatorController == null) return;

        AnimatorStateInfo stateInfo = placeholderAnimator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.normalizedTime >= 1.0f && !placeholderAnimator.IsInTransition(0))
        {
            PickNextController();
        }
    }

    void LateUpdate()
    {
        if (placeholderAnimator != null)
        {
            // Apply Scale
            placeholderAnimator.transform.localScale = placeholderScale;

            // Apply Transparency (Alpha)
            if (placeholderRenderer != null)
            {
                Color color = placeholderRenderer.color;
                color.a = alpha;
                placeholderRenderer.color = color;
            }
        }
    }

    void PickNextController()
    {
        float totalWeight = 0;
        foreach (var entry in controllerList) totalWeight += entry.weight;

        if (totalWeight <= 0) return;

        float randomValue = Random.Range(0, totalWeight);
        float currentSum = 0;

        foreach (var entry in controllerList)
        {
            currentSum += entry.weight;
            if (randomValue <= currentSum)
            {
                placeholderAnimator.runtimeAnimatorController = entry.controller;
                placeholderAnimator.Play(0, -1, 0f);
                break;
            }
        }
    }
}
