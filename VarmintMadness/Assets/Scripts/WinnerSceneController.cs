using UnityEngine;
using TMPro;

public class WinnerSceneController : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform walkTarget;
    public TextMeshProUGUI winnerNameText;

    private GameObject winnerInstance;
    private Animator winnerAnimator;

    public float walkSpeed = 2f;

    private void Start()
    {
        // Display winner name
        winnerNameText.text = WinnerData.WinnerName + " Wins!";

        // Spawn the winner's character
        if (WinnerData.WinnerPrefab != null)
        {
            winnerInstance = Instantiate(WinnerData.WinnerPrefab, spawnPoint.position, Quaternion.identity);

            // Remove scripts that shouldn't run in the win scene
            Destroy(winnerInstance.GetComponent<PlayerMovement>());
            Destroy(winnerInstance.GetComponent<Rigidbody>());
            Destroy(winnerInstance.GetComponent<Collider>());

            winnerAnimator = winnerInstance.GetComponent<Animator>();

            if (winnerAnimator != null)
                winnerAnimator.SetBool("Running", true);
        }
    }

    private void Update()
    {
        if (winnerInstance == null) return;

        // Move toward the camera target
        winnerInstance.transform.position = Vector3.MoveTowards(
            winnerInstance.transform.position,
            walkTarget.position,
            walkSpeed * Time.deltaTime
        );
    }
}
