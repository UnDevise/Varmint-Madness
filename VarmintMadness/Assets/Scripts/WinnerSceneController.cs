using UnityEngine;
using TMPro;

public class WinnerSceneController : MonoBehaviour
{
    public TextMeshProUGUI winnerNameText;
    public float moveSpeed = 2f;

    private GameObject winnerInstance;
    private Vector3 targetPosition;

    private void Start()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No Main Camera found in WinScene.");
            return;
        }

        // Show winner name
        winnerNameText.text = WinnerData.WinnerName + " Wins!";

        if (WinnerData.WinnerSprite == null)
        {
            Debug.LogError("WinnerData.WinnerSprite is null.");
            return;
        }

        // World positions based on camera viewport
        Vector3 spawnWorld = cam.ViewportToWorldPoint(new Vector3(0.5f, -0.2f, 10f));
        targetPosition = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.3f, 10f));

        // Create a new object for the winner sprite
        winnerInstance = new GameObject("WinnerSprite");
        var sr = winnerInstance.AddComponent<SpriteRenderer>();
        sr.sprite = WinnerData.WinnerSprite;
        sr.sortingOrder = 100;

        winnerInstance.transform.position = spawnWorld;
    }

    private void Update()
    {
        if (winnerInstance == null) return;

        winnerInstance.transform.position = Vector3.MoveTowards(
            winnerInstance.transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }
}
