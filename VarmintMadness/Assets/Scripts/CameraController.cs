using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private Camera cam;
    private Vector3 fullViewPosition;
    private float fullViewOrthographicSize;

    [Header("Zoom Settings")]
    public float zoomedOrthographicSize = 3.0f;
    public float zoomSpeed = 2.0f;
    public float followSpeed = 5.0f;

    private bool isFollowingPlayer = false;
    private Transform playerToFollow;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (cam.orthographic)
        {
            fullViewOrthographicSize = cam.orthographicSize;
        }
        fullViewPosition = transform.position;
    }

    private void Update()
    {
        if (isFollowingPlayer && playerToFollow != null)
        {
            FollowPlayer();
        }
    }

    private void FollowPlayer()
    {
        Vector3 targetPosition = new Vector3(
            playerToFollow.position.x,
            playerToFollow.position.y,
            transform.position.z
        );
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    // -------------------------------
    // NEW: Focus on Dice
    // -------------------------------
    public void FocusOnDice(Transform diceTransform)
    {
        StopAllCoroutines();
        isFollowingPlayer = false;
        playerToFollow = null;

        StartCoroutine(FocusDiceCoroutine(diceTransform));
    }

    private IEnumerator FocusDiceCoroutine(Transform dice)
    {
        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * zoomSpeed;

            // Zoom in
            cam.orthographicSize = Mathf.Lerp(startSize, zoomedOrthographicSize, elapsed);

            // Move camera toward dice
            Vector3 targetPos = new Vector3(dice.position.x, dice.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPos, elapsed);

            yield return null;
        }

        cam.orthographicSize = zoomedOrthographicSize;
    }

    // -------------------------------
    // NEW: Focus on Player After Dice Roll
    // -------------------------------
    public void FocusOnPlayer(Transform playerTransform)
    {
        StopAllCoroutines();
        StartCoroutine(StartFollowingCoroutine(playerTransform));
    }

    // Existing coroutine (unchanged)
    public IEnumerator StartFollowingCoroutine(Transform playerTransform)
    {
        playerToFollow = playerTransform;

        float startOrthographicSize = cam.orthographicSize;
        float elapsedTime = 0;

        while (elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime * zoomSpeed;
            cam.orthographicSize = Mathf.Lerp(startOrthographicSize, zoomedOrthographicSize, elapsedTime);

            Vector3 targetPosition = new Vector3(playerToFollow.position.x, playerToFollow.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime);

            yield return null;
        }

        cam.orthographicSize = zoomedOrthographicSize;
        isFollowingPlayer = true;
    }

    public void StopFollowing()
    {
        isFollowingPlayer = false;
        playerToFollow = null;
        StartCoroutine(ReturnToFullView());
    }

    public void SwitchPlayer(Transform newPlayer)
    {
        StopAllCoroutines();
        isFollowingPlayer = false;
        playerToFollow = null;

        StartCoroutine(StartFollowingCoroutine(newPlayer));
    }

    private IEnumerator ReturnToFullView()
    {
        while (Mathf.Abs(cam.orthographicSize - fullViewOrthographicSize) > 0.01f)
        {
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(
                    cam.orthographicSize,
                    fullViewOrthographicSize,
                    zoomSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        if (cam.orthographic)
        {
            cam.orthographicSize = fullViewOrthographicSize;
        }
    }
}