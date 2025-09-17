using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class UIDemo : MonoBehaviour
{
    public TextMeshProUGUI output;
    public TMP_InputField playerAmount;

    public void ButtonPlayer()
    {
        output.text = "Welcome Players " + playerAmount.text;
    }

}
