using UnityEngine;

public class AnimationOffset : MonoBehaviour
{
    private Animator animator;

    [Header("Animation Settings")]
    [Tooltip("The exact name of the animation state in the Animator window.")]
    public string stateName = "Idle";

    [Range(0f, 1f)]
    [Tooltip("0 is the start, 1 is the end (e.g., 0.5 starts halfway through).")]
    public float timeOffset = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            // Plays the state starting at the specified normalized time
            // Parameters: (stateName, layerIndex, normalizedTime)
            animator.Play(stateName, -1, timeOffset);
        }
    }
}
