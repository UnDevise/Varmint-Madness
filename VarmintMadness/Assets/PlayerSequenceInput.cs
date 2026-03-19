using UnityEngine;
using System.Collections.Generic;

public class PlayerSequenceInput : MonoBehaviour
{
    public int playerIndex;

    private int expectedInputs;
    private int currentInputIndex;

    public void BeginInput(int count)
    {
        expectedInputs = count;
        currentInputIndex = 0;
    }

    public bool RegisterInput(int padID, List<int> sequence)
    {
        if (padID != sequence[currentInputIndex])
            return false;

        currentInputIndex++;
        return true;
    }

    public bool HasCompletedSequence()
    {
        return currentInputIndex >= expectedInputs;
    }
}