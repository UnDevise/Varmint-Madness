using UnityEngine;

public static class BoardStateSaver
{
    public static string lastBoardSceneName = "";
    public static bool returningFromMinigame = false;

    public static Vector3[] playerPositions;
    public static int[] playerBoardLayer;
    public static int[] playerTileIndex;
    public static bool[] playerIsStunned;
    public static bool[] playerIsInCage;
    public static int[] playerGarbageCounts;
    public static int[] playerCharacterIndices;

    public static void Clear()
    {
        lastBoardSceneName = "";
        returningFromMinigame = false;

        playerPositions = null;
        playerBoardLayer = null;
        playerTileIndex = null;
        playerIsStunned = null;
        playerIsInCage = null;
        playerGarbageCounts = null;
        playerCharacterIndices = null;
    }

public static void RestorePlayerPositions()
    {
        if (playerPositions == null) return;

        PlayerMovement[] players = GameObject.FindObjectsOfType<PlayerMovement>();
        int count = Mathf.Min(players.Length, playerPositions.Length);

        for (int i = 0; i < count; i++)
        {
            players[i].transform.position = playerPositions[i];
            players[i].CurrentBoardLayer = playerBoardLayer[i];
            players[i].CurrentPositionIndex = playerTileIndex[i];
            players[i].IsStunned = playerIsStunned[i];
            players[i].IsInCage = playerIsInCage[i];
        }
    }

    public static void SaveBoardState() { }

    public static void RestoreBoardState() { }
}