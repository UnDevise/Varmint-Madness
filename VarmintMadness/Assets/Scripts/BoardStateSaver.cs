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

    // Round tracking
    public static int totalRounds = 5;
    public static int currentRound = 0;
    public static bool lastManStanding = false; // If true, play until one player remains

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

        // Don't clear round data here - it needs to persist across minigame trips
    }

    public static void ResetRounds()
    {
        currentRound = 0;
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