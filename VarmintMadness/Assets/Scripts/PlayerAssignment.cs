using UnityEngine;

public class PlayerAssignment : MonoBehaviour
{
    public PlayerMovement[] players;

    void Start()
    {
        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);
        Debug.Log($"TotalPlayers = {totalPlayers}");

        for (int i = 0; i < totalPlayers; i++)
        {
            int characterIndex = PlayerPrefs.GetInt("P" + (i + 1) + "_Character", 0);

            PlayerMovement pm = players[i];

            Debug.Log($"{pm.name}: characterIndex = {characterIndex}, renderer = {pm.characterRenderer}");

            pm.playerId = "P" + (i + 1);
            pm.ApplyCharacter(characterIndex);
        }
    }
}