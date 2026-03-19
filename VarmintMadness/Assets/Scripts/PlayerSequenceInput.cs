using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSequenceInput : MonoBehaviour
{
    public int playerIndex;

    private int expectedInputs;
    private int currentInputIndex;

    private Vector3 startPosition;
    private SecretSequenceManager manager;

    public bool movementFinished = false;

    void Start()
    {
        startPosition = transform.position;
        manager = FindObjectOfType<SecretSequenceManager>();
    }

    public void BeginInput(int count)
    {
        expectedInputs = count;
        currentInputIndex = 0;
        movementFinished = true; // movement already finished when this is called
    }

    public bool RegisterInput(int padID, List<int> sequence)
    {
        if (!movementFinished)
            return false;

        if (currentInputIndex < 0 || currentInputIndex >= sequence.Count)
            return false;

        if (padID != sequence[currentInputIndex])
            return false;

        currentInputIndex++;
        return true;
    }

    public bool HasCompletedSequence()
    {
        return currentInputIndex >= expectedInputs;
    }

    public void MoveToTurnPosition(Vector3 target)
    {
        movementFinished = false;
        StopAllCoroutines();
        StartCoroutine(MoveTo(target, notifyManager: true));
    }

    public void ReturnToStartPosition()
    {
        movementFinished = false;
        StopAllCoroutines();
        StartCoroutine(MoveTo(startPosition, notifyManager: false));
    }

    IEnumerator MoveTo(Vector3 target, bool notifyManager)
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                5f * Time.deltaTime
            );
            yield return null;
        }

        transform.position = target;

        if (notifyManager)
        {
            manager.OnPlayerFinishedMoving();
        }
    }
}