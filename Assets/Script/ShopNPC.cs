using UnityEngine;
using System.Collections.Generic;

public class ShopNPC : MonoBehaviour
{
    [SerializeField] private List<ItemData> shopInventory;

    public void OnShop()
    {
        GameManager.Instance.ChangeState(GameState.Shop);
        ShopManager.Instance.OpenShop(shopInventory);
    }

    public void OnSell()
    {
        ShopManager.Instance.OpenSellShop();
    }
}
