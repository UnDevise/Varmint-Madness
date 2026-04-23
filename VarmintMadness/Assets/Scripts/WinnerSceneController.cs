using UnityEngine;
using TMPro;

public class WinnerSceneController : MonoBehaviour
{
    public TextMeshProUGUI winnerNameText;
    public float moveSpeed = 2f;

    private GameObject winnerInstance;
    private Vector3 targetPosition;

    [SerializeField] private string winnerName; // visible in Inspector during Play Mode

    private void Start()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No Main Camera found in WinScene.");
            return;
        }

        // Load winner name
        winnerName = WinnerData.WinnerName;
        winnerNameText.text = winnerName + " Wins!";

        // Load winner sprite
        if (WinnerData.WinnerSprite == null)
        {
            Debug.LogError("WinnerData.WinnerSprite is null. Did you forget to set it before loading the scene?");
            return;
        }

        // Spawn and target positions
        Vector3 spawnWorld = cam.ViewportToWorldPoint(new Vector3(0.5f, -0.2f, 10f));
        targetPosition = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.3f, 10f));

        // Create winner sprite object
        winnerInstance = new GameObject("WinnerSprite");
        var sr = winnerInstance.AddComponent<SpriteRenderer>();
        sr.sprite = WinnerData.WinnerSprite;
        sr.sortingOrder = 100;

        winnerInstance.transform.position = spawnWorld;
    }

    private bool themePlayed = false;

    private void Update()
    {
        if (winnerInstance == null) return;

        winnerInstance.transform.position = Vector3.MoveTowards(
            winnerInstance.transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // ⭐ Check if winner reached the target
        if (!themePlayed &&
            Vector3.Distance(winnerInstance.transform.position, targetPosition) < 0.05f)
        {
            themePlayed = true;

            Debug.Log("Winner reached target — triggering theme");

            WinnerThemePlayer.Instance.PlayWinnerTheme();
        }
    }
}