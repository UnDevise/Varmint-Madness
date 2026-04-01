using UnityEngine;

public class PointerDetector : MonoBehaviour
{
    public string currentColor = "";

    private void OnTriggerStay2D(Collider2D collision)
    {
        SpinnerSlice slice = collision.GetComponent<SpinnerSlice>();
        if (slice != null)
        {
            currentColor = slice.sliceColor;
        }
    }
}
