using UnityEngine;

public static class BoardStateSaver
{
    public static string lastBoardSceneName;
    public static string nextMinigameScene;

    public static Vector3[] playerPositions;
    public static int[] playerBoardLayer;
    public static int[] playerTileIndex;
    public static bool[] playerIsStunned;
    public static bool[] playerIsInCage;
    public static int[] playerGarbageCounts;

    public static void SavePlayerPositions()
    {
        PlayerMovement[] players = GameObject.FindObjectsOfType<PlayerMovement>();
        int count = players.Length;

        playerPositions = new Vector3[count];
        playerBoardLayer = new int[count];
        playerTileIndex = new int[count];
        playerIsStunned = new bool[count];
        playerIsInCage = new bool[count];

        for (int i = 0; i < count; i++)
        {
            playerPositions[i] = players[i].transform.position;
            playerBoardLayer[i] = players[i].CurrentBoardLayer;
            playerTileIndex[i] = players[i].CurrentPositionIndex;
            playerIsStunned[i] = players[i].IsStunned;
            playerIsInCage[i] = players[i].IsInCage;
        }
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