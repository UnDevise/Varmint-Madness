using UnityEngine;

public class PlayerAssignment : MonoBehaviour
{
    public PlayerMovement[] players;

    void Start()
    {
        Debug.Log("Loaded P1_Character = " + PlayerPrefs.GetInt("P1_Character", -1));
        Debug.Log("Loaded P2_Character = " + PlayerPrefs.GetInt("P2_Character", -1));
        Debug.Log("Loaded P3_Character = " + PlayerPrefs.GetInt("P3_Character", -1));
        Debug.Log("Loaded P4_Character = " + PlayerPrefs.GetInt("P4_Character", -1));

        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);
        Debug.Log($"TotalPlayers = {totalPlayers}");

        for (int i = 0; i < totalPlayers; i++)
        {
            int characterIndex = PlayerPrefs.GetInt("P" + (i + 1) + "_Character", -1);

            PlayerMovement pm = players[i];

            Debug.Log($"{pm.name}: characterIndex = {characterIndex}, renderer = {pm.characterRenderer}");

            pm.playerId = "P" + (i + 1);
            pm.ApplyCharacter(characterIndex);

            if (characterIndex == -1)
            {
                Debug.LogWarning($"Player {i + 1} has no saved character — skipping ApplyCharacter.");
            }
            else
            {
                pm.ApplyCharacter(characterIndex);
            }

        }
    }
}