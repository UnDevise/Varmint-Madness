using UnityEngine;

public class ScenarioGenerator : MonoBehaviour
{
    [Header("Random Lists")]
    public string[] characters;
    public string[] places;
    public string[] actions;

    public string GenerateScenario()
    {
        string c = characters[Random.Range(0, characters.Length)];
        string p = places[Random.Range(0, places.Length)];
        string a = actions[Random.Range(0, actions.Length)];

        return $"{c} was in {p} when suddenly they {a}.";
    }
}