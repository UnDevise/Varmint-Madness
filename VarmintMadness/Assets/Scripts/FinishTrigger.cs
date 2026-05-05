using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    public MarathonMinigameController controller;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<RunnerController>())
        {
            controller.PlayerFinished();
        }
    }
}
