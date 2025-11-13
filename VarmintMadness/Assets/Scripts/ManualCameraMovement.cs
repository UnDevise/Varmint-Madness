using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 20f;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private bool followPlayer = true;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            followPlayer = !followPlayer;
        }

        if (followPlayer && player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
        }
        else
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, vertical, 0f);
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        // Clamp camera position
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
        transform.position = clampedPosition;

        // Zoom controls
        if (Input.GetKey(KeyCode.Z))
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        }
        if (Input.GetKey(KeyCode.X))
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        }
    }
}




