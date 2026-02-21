using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SquareSpawnData
{
    public GameObject squarePrefab;
    public int count;
}

public class SquareSpawner : MonoBehaviour
{
    public SquareSpawnData[] squaresToSpawn;
    private List<Transform> placeholderPositions;

    void Start()
    {
        GameObject[] placeholders = GameObject.FindGameObjectsWithTag("Placeholder");
        placeholderPositions = new List<Transform>();
        foreach (GameObject placeholder in placeholders)
        {
            placeholderPositions.Add(placeholder.transform);
        }

        if (placeholderPositions.Count > 0)
        {
            SpawnSquares();
        }
        else
        {
            Debug.LogError("No placeholders with the 'Placeholder' tag were found!");
        }

        for (int i = 0; i < placeholders.Length; i++)
        {
            Destroy(placeholders[i]);
        }
    }

    void SpawnSquares()
    {
        List<Transform> availablePositions = new List<Transform>(placeholderPositions);

        // Iterate through each square type and spawn the specified count
        foreach (SquareSpawnData spawnData in squaresToSpawn)
        {
            for (int i = 0; i < spawnData.count; i++)
            {
                if (availablePositions.Count > 0)
                {
                    // Pick a random available placeholder position
                    int randomPositionIndex = Random.Range(0, availablePositions.Count);
                    Transform spawnPoint = availablePositions[randomPositionIndex];

                    // Instantiate the square prefab
                    Instantiate(spawnData.squarePrefab, spawnPoint.position, Quaternion.identity);

                    // Remove the used position
                    availablePositions.RemoveAt(randomPositionIndex);
                }
                else
                {
                    Debug.LogWarning("Not enough placeholders for all squares!");
                    break;
                }
            }
        }
    }
}
