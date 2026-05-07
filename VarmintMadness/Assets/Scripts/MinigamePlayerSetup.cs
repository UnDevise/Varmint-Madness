using UnityEngine;
using TMPro;

// Attach this to a GameObject in each minigame scene.
// Assign all 4 player objects and their UI elements in order.
// It will automatically disable unused players based on TotalPlayers from PlayerPrefs.

public class MinigamePlayerSetup : MonoBehaviour
{
    [Header("Assign all players in order (1-4)")]
    public GameObject[] playerObjects;

    [Header("Optional: UI text per player (score, name, etc.)")]
    public GameObject[] playerUIElements;

    [Header("Optional: Start points per player")]
    public Transform[] playerStartPoints;

    void Awake()
    {
        int totalPlayers = PlayerPrefs.GetInt("TotalPlayers", 4);

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i] == null) continue;

            bool isActive = i < totalPlayers;
            playerObjects[i].SetActive(isActive);

            // Move active players to their start points
            if (isActive && playerStartPoints != null && i < playerStartPoints.Length && playerStartPoints[i] != null)
                playerObjects[i].transform.position = playerStartPoints[i].position;
        }

        // Hide UI for unused players
        if (playerUIElements != null)
        {
            for (int i = 0; i < playerUIElements.Length; i++)
            {
                if (playerUIElements[i] != null)
                    playerUIElements[i].SetActive(i < totalPlayers);
            }
        }
    }
}
