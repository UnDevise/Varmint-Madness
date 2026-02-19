using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class BotSlot
    {
        public GameObject selectionUI;
        public Button addBotButton;
        public Button removeButton;
    }

    public List<BotSlot> botSlots; // Drag Bot1, Bot2, Bot3 prefabs here
    private int activeBots = 0;

    void Start()
    {
        RefreshUI();
    }

    public void AddBot()
    {
        if (activeBots < botSlots.Count)
        {
            activeBots++;
            RefreshUI();
        }
    }

    public void RemoveBot()
    {
        if (activeBots > 0)
        {
            activeBots--;
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        for (int i = 0; i < botSlots.Count; i++)
        {
            // Show Selection UI if bot is active
            botSlots[i].selectionUI.SetActive(i < activeBots);

            // "Add Bot" button only shows for the NEXT available slot
            botSlots[i].addBotButton.gameObject.SetActive(i == activeBots);

            // "Remove" button only shows on the highest active bot
            if (botSlots[i].removeButton != null)
            {
                botSlots[i].removeButton.gameObject.SetActive(i == activeBots - 1);
            }
        }
    }
}
