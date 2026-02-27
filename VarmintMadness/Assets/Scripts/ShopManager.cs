using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public GameObject shopUI;

    private bool isOpen = false;

    public void ToggleShop()
    {
        isOpen = !isOpen;
        shopUI.SetActive(isOpen);
    }

    public void CloseShop()
    {
        isOpen = false;
        shopUI.SetActive(false);
    }
    public void BuySpeedBoost()
    {
        DiceController dice = FindAnyObjectByType<DiceController>();
        PlayerMovement player = dice.playersToMove[dice.currentPlayerIndex];

        if (player.GetGarbage() >= 5)
        {
            player.SpendGarbage(5);
            player.moveSpeed += 2f;
            Debug.Log(player.playerName + " + 5 spaces!");
        }
        else
        {
            Debug.Log("Not enough garbage!");
        }
    }
    public void BuyMoveRandomPlayer()
    {
        DiceController dice = FindAnyObjectByType<DiceController>();
        PlayerMovement player = dice.playersToMove[dice.currentPlayerIndex];

        if (player.GetGarbage() >= 15)
        {
            player.SpendGarbage(15);
            Debug.Log(player.playerName + " bought Move Random Player!");

            dice.MoveRandomPlayer();   // ⭐ Trigger the ability
        }
        else
        {
            Debug.Log("Not enough garbage!");
        }
    }
}
