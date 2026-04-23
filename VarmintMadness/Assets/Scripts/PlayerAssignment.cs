using UnityEngine;

public class PlayerAssignment : MonoBehaviour
{
    public PlayerMovement[] players; // Assign all 4 in Inspector

    void Start()
    {
        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);
        Debug.Log($"TotalPlayers = {totalPlayers}");

        for (int i = 0; i < players.Length; i++)
        {
            PlayerMovement pm = players[i];

            if (i < totalPlayers)
            {
                // ENABLE active players
                pm.gameObject.SetActive(true);

                // Correct playerID (1–4)
                pm.playerID = i + 1;

                // Load saved character
                int characterIndex = PlayerPrefs.GetInt($"P{pm.playerID}_Character", -1);

                Debug.Log($"Assigning Player {pm.playerID} → Character {characterIndex}");

                if (characterIndex != -1)
                    pm.ApplyCharacter(characterIndex);
            }
            else
            {
                // DISABLE unused players
                pm.gameObject.SetActive(false);
            }
        }
    }
}
