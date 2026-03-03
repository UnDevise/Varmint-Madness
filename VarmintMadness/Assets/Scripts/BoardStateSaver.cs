using UnityEngine;

public static class BoardStateSaver
{
    public static int[] playerBoardLayer;   // 0 = top board, 1 = sewer
    public static int[] playerTileIndex;    // tile index on path
    public static bool[] playerIsStunned;   // skip-turn state
    public static bool[] playerIsInCage;    // cage state

    public static void SaveBoardState(int playerCount, DiceController dice)
    {
        playerBoardLayer = new int[playerCount];
        playerTileIndex = new int[playerCount];
        playerIsStunned = new bool[playerCount];
        playerIsInCage = new bool[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            PlayerMovement p = dice.playersToMove[i];

            // Board layer
            bool isOnSewer = (p.waypointsParent == p.alternativeWaypointsParent);
            playerBoardLayer[i] = isOnSewer ? 1 : 0;

            // Tile index (safe)
            playerTileIndex[i] = p.GetCurrentTileIndex();

            // Stun / cage
            playerIsStunned[i] = p.IsStunned;
            playerIsInCage[i] = p.IsInCage;
        }
    }
}