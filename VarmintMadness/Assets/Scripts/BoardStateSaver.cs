using UnityEngine;

public static class BoardStateSaver
{
    // 0 = Top Board, 1 = Sewer Board
    public static int[] playerBoardLayer;

    public static void SaveBoardState(int playerCount, DiceController dice)
    {
        playerBoardLayer = new int[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            PlayerMovement p = dice.playersToMove[i];

            // Sewer board is represented by alternativeWaypointsParent
            bool isOnSewer = (p.waypointsParent == p.alternativeWaypointsParent);

            playerBoardLayer[i] = isOnSewer ? 1 : 0;
        }
    }
}
