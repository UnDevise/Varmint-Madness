using UnityEngine;


public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject shopUI;

    // Whether the shop is currently open
    public bool shopOpen = false;

    public void OpenShop()
    {
        if (shopUI != null)
            shopUI.SetActive(true);

        shopOpen = true;
    }

    public void CloseShop()
    {
        if (shopUI != null)
            shopUI.SetActive(false);

        shopOpen = false;
    }

    // Optional: toggle method if you want to call just one function
    public void ToggleShop()
    {
        if (shopOpen)
            CloseShop();
        else
            OpenShop();
    }
}
