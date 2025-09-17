using UnityEngine;
using TMPro;

public class UIDemo : MonoBehaviour
{
    public TextMeshProUGUI output;
    public TMP_InputField playerAmount;

    public void ButtonPlayer()
    {
        int players;

        // Try converting input text into a number
        if (int.TryParse(playerAmount.text, out players))
        {
            // Clamp the number between 1 and 4
            players = Mathf.Clamp(players, 1, 4);

            // Show output
            output.text = "Welcome Players " + players;
        }
        else
        {
            // Invalid input (like letters)
            output.text = "Please enter a number between 1 and 4.";
        }
    }
}
