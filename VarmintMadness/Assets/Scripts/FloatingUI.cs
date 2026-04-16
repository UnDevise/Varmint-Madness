using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    public float amplitude = 20f;   // how far it moves up/down
    public float speed = 1f;        // how fast it moves

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);
    }
}