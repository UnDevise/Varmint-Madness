using UnityEngine;

public class GameManagerMarble : MonoBehaviour
{
    public MarbleMovement[] marbles; // Assign all marbles in Inspector
    private int playersChosen = 0;
    public int totalPlayers = 2; // Set number of players

    // Called when a player clicks a marble
    public void PlayerChoseMarble()
    {
        playersChosen++;

        // Once all players have chosen, start the race
        if (playersChosen >= totalPlayers)
        {
            StartRace();
        }
    }

    private void StartRace()
    {
        foreach (MarbleMovement marble in marbles)
        {
            marble.StartRace(); // Start ALL marbles, even unselected ones
        }
    }
}


