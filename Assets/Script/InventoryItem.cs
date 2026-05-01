using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int amount;

    public InventoryItem(ItemData data, int amount)
    {
        this.itemData = data;
        this.amount = amount;
    }
}
