using UnityEngine;

public static class ItemDisplayHelper
{
    public static int GetDisplayAmount(InventoryItem item)
    {
        if(item == null || item.itemData == null) return 0;

        int amount = item.amount;

        if(EquipmentManager.Instance != null &&
            item.itemData.equipData != null)
        {
            var equipType = item.itemData.equipData.equipType;

            if(equipType != EquipData.EquipType.None)
            {
                var equipped = EquipmentManager.Instance.GetEquipped(equipType);

                if(equipped == item.itemData)
                {
                    amount -= 1;
                }
            }
        }

        return Mathf.Max(0, amount);
    }

    public static bool IsEquipped(InventoryItem item)
    {
        if(item == null || item.itemData == null) return false;

        if(EquipmentManager.Instance == null) return false;

        if(item.itemData.equipData == null) return false;

        var equipType = item.itemData.equipData.equipType;

        if(equipType == EquipData.EquipType.None) return false;

        var equipped = EquipmentManager.Instance.GetEquipped(equipType);

        return equipped == item.itemData;
    }
}
