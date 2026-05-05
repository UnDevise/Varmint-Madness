using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    public MarathonMinigameController controller;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Finish Trigger hit by: " + other.name);

        RunnerController runner = other.GetComponent<RunnerController>();

        if (runner != null)
        {
            Debug.Log("Runner canFinish = " + runner.canFinish);
        }

        if (runner != null && runner.canFinish)
        {
            Debug.Log("FINISH CONFIRMED");
            controller.PlayerFinished();
        }
    }

}