using UnityEngine;

public class ButtonSpriteController : MonoBehaviour
{
    public Animator animator;
    public ParticleSystem explosionFX;

    public void PlayPressAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Press");
    }

    public void PlayExplosionFX()
    {
        if (explosionFX != null)
            explosionFX.Play();
    }

    public void DisableButton()
    {
        gameObject.SetActive(false);
    }
}