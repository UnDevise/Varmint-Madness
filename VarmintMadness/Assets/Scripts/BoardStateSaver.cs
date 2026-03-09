using UnityEngine;

public static class BoardStateSaver
{
    public static Vector3[] savedPositions;
    public static bool[] playerIsInCage;
    public static int[] savedGarbageCounts;
    public static int savedCurrentPlayerIndex;
    public static string lastBoardSceneName;

    // ⭐ These were missing — PlayerMovement requires them
    public static int[] playerBoardLayer;
    public static int[] playerTileIndex;
    public static bool[] playerIsStunned;

    public static void SaveBoardState(int playerCount, DiceController dice)
    {
        savedPositions = new Vector3[playerCount];
        playerIsInCage = new bool[playerCount];
        savedGarbageCounts = new int[playerCount];
        playerBoardLayer = new int[playerCount];
        playerTileIndex = new int[playerCount];
        playerIsStunned = new bool[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            PlayerMovement p = dice.playersToMove[i];

            savedPositions[i] = p.transform.position;
            playerIsInCage[i] = p.IsInCage;
            savedGarbageCounts[i] = p.garbageCount;

            // ⭐ PlayerMovement requires these
            playerBoardLayer[i] = (p.waypointsParent == p.alternativeWaypointsParent) ? 1 : 0;
            playerTileIndex[i] = p.GetCurrentTileIndex();
            playerIsStunned[i] = p.IsStunned;
        }

        savedCurrentPlayerIndex = dice.currentPlayerIndex;
    }

}