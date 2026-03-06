using System.Collections.Generic;
using UnityEngine;

public class OffscreenIndicatorManager : MonoBehaviour
{
    public static OffscreenIndicatorManager Instance;

    [Header("Settings")]
    public Camera targetCamera;
    public bool showIndicators = true;

    [Header("UI")]
    public RectTransform indicatorParent;
    public GameObject indicatorPrefab;

    private Dictionary<Transform, OffscreenIndicator> indicators = new Dictionary<Transform, OffscreenIndicator>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        foreach (var pair in indicators)
        {
            if (pair.Value != null)
            {
                pair.Value.UpdateIndicator(showIndicators);
            }
        }
    }

    public void AddTarget(Transform target, Sprite sprite, Color arrowColor)
    {
        if (indicators.ContainsKey(target)) return;

        GameObject obj = Instantiate(indicatorPrefab, indicatorParent);
        OffscreenIndicator indicator = obj.GetComponent<OffscreenIndicator>();

        indicator.Initialize(target, sprite, arrowColor, targetCamera);

        indicators.Add(target, indicator);
    }

    public void RemoveTarget(Transform target)
    {
        if (!indicators.ContainsKey(target)) return;

        Destroy(indicators[target].gameObject);
        indicators.Remove(target);
    }

    public void ToggleIndicators(bool value)
    {
        showIndicators = value;
    }
}