using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResultsScreenManager : MonoBehaviour
{
    public static ResultsScreenManager Instance;

    [Header("UI")]
    public GameObject resultsPanel;
    public Transform resultsContainer;
    public GameObject resultEntryPrefab;

    private void Awake()
    {
        Instance = this;
        resultsPanel.SetActive(false);
    }

    public void ShowResults(List<PlayerMovement> players)
    {
        resultsPanel.SetActive(true);

        // Clear old entries
        foreach (Transform child in resultsContainer)
            Destroy(child.gameObject);

        foreach (PlayerMovement p in players)
        {
            GameObject entry = Instantiate(resultEntryPrefab, resultsContainer);

            // UI references
            TextMeshProUGUI nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI trashText = entry.transform.Find("TrashText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI reasonText = entry.transform.Find("ReasonText").GetComponent<TextMeshProUGUI>();
            Image portrait = entry.transform.Find("Portrait").GetComponent<Image>();

            // Fill in data
            nameText.text = p.playerName;
            trashText.text = $"Trash: {p.garbageCount}";
            portrait.sprite = p.characterRenderer.sprite;

            // Determine elimination reason
            reasonText.text = GetEliminationReason(p);
        }
    }

    private string GetEliminationReason(PlayerMovement p)
    {
        if (p.garbageCount <= 0)
            return "Eliminated: Ran out of trash";

        if (p.IsInCage)
            return "Eliminated: Locked in the cage";

        if (p.IsStunned)
            return "Eliminated: Stunned";

        return "Winner!";
    }
}
