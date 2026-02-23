using UnityEngine;

public static class BoardStateSaver
{
    // 0 = Top Board, 1 = Sewer Board
    public static int[] playerBoardLayer;

    // Save each player's tile index
    public static int[] playerTileIndex;

    public static void SaveBoardState(int playerCount, DiceController dice)
    {
        playerBoardLayer = new int[playerCount];
        playerTileIndex = new int[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            PlayerMovement p = dice.playersToMove[i];

            // Save board layer
            bool isOnSewer = (p.waypointsParent == p.alternativeWaypointsParent);
            playerBoardLayer[i] = isOnSewer ? 1 : 0;

            // Save tile index
            playerTileIndex[i] = p.GetCurrentTileIndex();
        }
    }
}

