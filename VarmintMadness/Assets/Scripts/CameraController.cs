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

    public enum CameraMode
    {
        None,
        FollowPlayer,
        FocusDice
    }

    public CameraMode currentMode = CameraMode.None;


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
        if (playerToFollow == null)
            return;

        Vector3 targetPosition = new Vector3(
            playerToFollow.position.x,
            playerToFollow.position.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    public void FocusOnDice(Transform diceTransform)
    {
        if (diceTransform == null)
            return;

        currentMode = CameraMode.FocusDice;
        StopAllCoroutines();
        isFollowingPlayer = false;
        playerToFollow = null;

        StartCoroutine(FocusDiceCoroutine(diceTransform));
    }

    private IEnumerator FocusDiceCoroutine(Transform dice)
    {
        if (dice == null)
            yield break;

        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < 1f)
        {
            if (dice == null)
                yield break;

            elapsed += Time.deltaTime * zoomSpeed;

            cam.orthographicSize = Mathf.Lerp(startSize, zoomedOrthographicSize, elapsed);

            Vector3 targetPos = new Vector3(
                dice.position.x,
                dice.position.y,
                transform.position.z
            );

            transform.position = Vector3.Lerp(transform.position, targetPos, elapsed);

            yield return null;
        }

        cam.orthographicSize = zoomedOrthographicSize;
    }

    public void FocusOnPlayer(Transform playerTransform)
    {
        if (playerTransform == null)
            return;

        currentMode = CameraMode.FollowPlayer;
        StopAllCoroutines();
        StartCoroutine(StartFollowingCoroutine(playerTransform));
    }

    public IEnumerator StartFollowingCoroutine(Transform playerTransform)
    {
        if (playerTransform == null)
            yield break;

        playerToFollow = playerTransform;

        float startSize = cam.orthographicSize;
        float elapsedTime = 0;

        while (elapsedTime < 1)
        {
            if (playerToFollow == null)
                yield break;

            elapsedTime += Time.deltaTime * zoomSpeed;

            cam.orthographicSize = Mathf.Lerp(startSize, zoomedOrthographicSize, elapsedTime);

            Vector3 targetPosition = new Vector3(
                playerToFollow.position.x,
                playerToFollow.position.y,
                transform.position.z
            );

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

    private IEnumerator ReturnToFullView()
    {
        while (Mathf.Abs(cam.orthographicSize - fullViewOrthographicSize) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                fullViewOrthographicSize,
                zoomSpeed * Time.deltaTime
            );

            yield return null;
        }

        cam.orthographicSize = fullViewOrthographicSize;
    }
}
