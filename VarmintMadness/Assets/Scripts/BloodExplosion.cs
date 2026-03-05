using UnityEngine;

public class ClickableBlood : MonoBehaviour
{
    [Header("Blood Effect")]
    public GameObject bloodParticlesPrefab;
    public AudioClip splatSound;

    private bool destroyed = false;

    void OnMouseDown()
    {
        // Prevent double clicking
        if (destroyed) return;

        destroyed = true;
        AudioSource.PlayClipAtPoint(splatSound, transform.position);

        SpawnBlood();
        Destroy(gameObject);
    }

    void SpawnBlood()
    {
        if (bloodParticlesPrefab != null)
        {
            GameObject blood = Instantiate(
                bloodParticlesPrefab,
                transform.position,
                Quaternion.identity
            );

            // Destroy particles after they finish
            Destroy(blood, 2f);
        }
    }
}