using UnityEngine;

public class ManualCameraMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float zoomSpeed = 2f;
    public float minZoom = 2f;
    public float maxZoom = 20f;

    private bool canMove = false;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            canMove = !canMove;
        }

        if (canMove)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, vertical, 0f);
            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Z))
        {
            cam.orthographicSize += zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }

        if (Input.GetKey(KeyCode.X))
        {
            cam.orthographicSize -= zoomSpeed * Time.deltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}


