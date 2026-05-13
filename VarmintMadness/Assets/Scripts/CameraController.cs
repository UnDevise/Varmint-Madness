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

    [Header("Look Around Settings")]
    public float lookMoveSpeed = 10f;
    public float lookZoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 25f;

    private bool isFollowingPlayer = false;
    private Transform playerToFollow;
    private bool followDice = false;
    private Transform diceToFollow;

    private Vector3 savedLookPosition;
    private float savedLookZoom;

    public enum CameraMode
    {
        None,
        FollowPlayer,
        FocusDice,
        LookAround
    }

    public CameraMode currentMode = CameraMode.None;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (cam.orthographic)
            fullViewOrthographicSize = cam.orthographicSize;

        fullViewPosition = transform.position;
    }

    private void Update()
    {
        // AUTO-UNLOCK IF TARGET IS GONE
        if (currentMode == CameraMode.FollowPlayer && playerToFollow == null)
        {
            isFollowingPlayer = false;
            currentMode = CameraMode.None;
        }

        if (currentMode == CameraMode.FocusDice && diceToFollow == null)
        {
            followDice = false;
            currentMode = CameraMode.None;
        }

        // CAMERA BEHAVIOR
        switch (currentMode)
        {
            case CameraMode.FollowPlayer:
                if (isFollowingPlayer && playerToFollow != null)
                    FollowPlayer();
                break;

            case CameraMode.FocusDice:
                if (followDice && diceToFollow != null)
                    FollowDice();
                break;

            case CameraMode.LookAround:
                HandleLookAroundMovement();
                HandleLookAroundZoom();
                break;
        }

        // Q TOGGLE
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (currentMode == CameraMode.FollowPlayer || currentMode == CameraMode.FocusDice)
                return;

            if (currentMode != CameraMode.LookAround)
                EnterLookAroundMode();
            else
                ExitLookAroundMode();
        }
    }

    // -----------------------------
    // LOOK AROUND MODE
    // -----------------------------
    public void EnterLookAroundMode()
    {
        if (currentMode == CameraMode.LookAround)
            return;

        savedLookPosition = transform.position;
        savedLookZoom = cam.orthographicSize;

        StopAllCoroutines();
        isFollowingPlayer = false;
        followDice = false;

        currentMode = CameraMode.LookAround;
    }

    public void ExitLookAroundMode()
    {
        currentMode = CameraMode.None;

        transform.position = savedLookPosition;
        cam.orthographicSize = savedLookZoom;
    }

    private void HandleLookAroundMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, v, 0f) * lookMoveSpeed * Time.deltaTime;
        transform.position += move;
    }

    private void HandleLookAroundZoom()
    {
        // Scroll wheel zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * lookZoomSpeed;

        // Left click = zoom in
        if (Input.GetMouseButton(0))
            cam.orthographicSize -= lookZoomSpeed * Time.deltaTime;

        // Right click = zoom out
        if (Input.GetMouseButton(1))
            cam.orthographicSize += lookZoomSpeed * Time.deltaTime;

        // Clamp zoom
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }

    // -----------------------------
    // PLAYER FOLLOW
    // -----------------------------
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

    // OLD API — restored
    public void StartFollowing(Transform target)
    {
        FocusOnPlayer(target);
    }

    public void FollowPlayer()
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

    public void FollowPlayer(Transform player)
    {
        FocusOnPlayer(player);
    }

    // -----------------------------
    // DICE FOCUS
    // -----------------------------
    public void FocusOnDice(Transform diceTransform)
    {
        if (diceTransform == null)
            return;

        currentMode = CameraMode.FocusDice;
        StopAllCoroutines();

        isFollowingPlayer = false;
        playerToFollow = null;

        followDice = true;                // ⭐ NEW
        diceToFollow = diceTransform;     // ⭐ NEW

        StartCoroutine(FocusDiceCoroutine(diceTransform));
    }

    public void FocusDice(Transform dice)
    {
        FocusOnDice(dice);
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

    private void FollowDice()
    {
        if (diceToFollow == null)
            return;

        Vector3 targetPosition = new Vector3(
            diceToFollow.position.x,
            diceToFollow.position.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    public void StopDiceFollow()
    {
        followDice = false;
        diceToFollow = null;
    }

    // -----------------------------
    // STOP FOLLOWING
    // -----------------------------
    public void StopFollowing()
    {
        isFollowingPlayer = false;
        playerToFollow = null;
        currentMode = CameraMode.None;

        StopAllCoroutines();
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