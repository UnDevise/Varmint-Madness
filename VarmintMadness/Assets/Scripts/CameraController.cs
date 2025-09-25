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

    // Public coroutine for starting the follow with a zoom transition.
    public IEnumerator StartFollowingCoroutine(Transform playerTransform)
    {
        playerToFollow = playerTransform;

        // Smoothly zoom in first.
        float startOrthographicSize = cam.orthographicSize;
        float elapsedTime = 0;

        while (elapsedTime < 1)
        {
            elapsedTime += Time.deltaTime * zoomSpeed;
            cam.orthographicSize = Mathf.Lerp(startOrthographicSize, zoomedOrthographicSize, elapsedTime);
            // Move camera towards the player while zooming
            Vector3 targetPosition = new Vector3(playerToFollow.position.x, playerToFollow.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime);
            yield return null;
        }

        cam.orthographicSize = zoomedOrthographicSize;

        // Start following once zoom is complete.
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
        // Smoothly return to the original position and zoom level.
        while (Vector3.Distance(transform.position, fullViewPosition) > 0.01f || Mathf.Abs(cam.orthographicSize - fullViewOrthographicSize) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, fullViewPosition, zoomSpeed * Time.deltaTime);
            if (cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, fullViewOrthographicSize, zoomSpeed * Time.deltaTime);
            }
            yield return null;
        }
        transform.position = fullViewPosition;
        if (cam.orthographic)
        {
            cam.orthographicSize = fullViewOrthographicSize;
        }
    }
}