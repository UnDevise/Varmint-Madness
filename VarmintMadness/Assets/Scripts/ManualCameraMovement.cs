using UnityEngine;

public class ManualCameraMovement : MonoBehaviour
{
    public Transform player;          // Assign your player GameObject in the Inspector
    public float moveSpeed = 5f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    private bool followPlayer = true;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // Toggle follow mode with Left Shift
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            followPlayer = !followPlayer;
        }

        // Follow player
        if (followPlayer && player != null)
        {
            Vector3 newPos = new Vector3(player.position.x, player.position.y, transform.position.z);
            transform.position = newPos;
        }

        // Manual movement when not following
        if (!followPlayer)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, vertical, 0f);
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        // Zoom out with Z
        if (Input.GetKey(KeyCode.Z))
        {
            cam.orthographicSize += zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }

        // Zoom in with X
        if (Input.GetKey(KeyCode.X))
        {
            cam.orthographicSize -= zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}



