using UnityEngine;

public class BoardSceneInitializer : MonoBehaviour
{
    void Start()
    {
        // Restore player positions
        BoardStateSaver.RestorePlayerPositions();

        // Restore turn order, dice state, etc.
        BoardStateSaver.RestoreBoardState();

        Debug.Log("BoardSceneInitializer: Board restored successfully.");
    }
}