using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    public MarathonMinigameController controller;

    private void OnTriggerEnter2D(Collider2D other)
    {
        RunnerController runner = other.GetComponent<RunnerController>();

        if (runner != null && runner.canFinish)
        {
            controller.PlayerFinished();
        }
    }
}