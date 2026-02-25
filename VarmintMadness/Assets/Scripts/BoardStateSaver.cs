using UnityEngine;

public static class BoardStateSaver
{
    public static int[] playerBoardLayer;     // 0 = top board, 1 = sewer
    public static int[] playerTileIndex;      // tile index
    public static bool[] playerIsStunned;     // skip-turn state
    public static bool[] playerIsInCage;      // cage state

    public static void SaveBoardState(int playerCount, DiceController dice)
    {
        playerBoardLayer = new int[playerCount];
        playerTileIndex = new int[playerCount];
        playerIsStunned = new bool[playerCount];
        playerIsInCage = new bool[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            PlayerMovement p = dice.playersToMove[i];

            // Save board layer (top or sewer)
            bool isOnSewer = (p.waypointsParent == p.alternativeWaypointsParent);
            playerBoardLayer[i] = isOnSewer ? 1 : 0;

            // Save tile index
            playerTileIndex[i] = p.GetCurrentTileIndex();

            // ⭐ Save skip-turn state
            playerIsStunned[i] = p.IsStunned;

            // ⭐ Save cage state
            playerIsInCage[i] = p.IsInCage;
        }
    }

}

