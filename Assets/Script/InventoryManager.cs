using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private List<InventoryItem> items = new List<InventoryItem>();

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddItem(ItemData data, int amount)
    {
        InventoryItem found = items.Find(i => i.itemData == data);

        if(found != null)
        {
            found.amount += amount;
        }
        else
        {
            items.Add(new InventoryItem(data,amount));
        }
        Debug.Log($"{data.itemName}‚ð{amount}ŒÂ“üŽè");
    }

    public void RemoveItem(ItemData data, int amount)
    {
        InventoryItem found = items.Find(i => i.itemData == data);

        if(found != null)
        {
            found.amount -= amount;

            if(found.amount <= 0)
            {
                items.Remove(found);
            }
        }
    }

    public int GetItemAmount(ItemData data)
    {
        var item = items.Find(i => i.itemData == data);
        if(item == null) return 0;
        return item.amount;
    }

    public int GetRemainingCapacity(ItemData data)
    {
        int current = GetItemAmount(data);
        return 99 - current;
    }

    public bool IsFull(ItemData itemData)
    {
        var item = items.Find(i => i.itemData == itemData);

        if(item == null) return false;

        return item.amount >= 99;
    }

    public List<InventoryItem> GetItems()
    {
        return items;
    }

    public List<ItemData> GetAllItems()
    {
        List<ItemData> result = new List<ItemData>();

        foreach( var item in items )
        {
            if(item.amount > 0 )
            {
                result.Add(item.itemData);
            }
        }
        return result;
    }

    public List<ItemSaveData> GetSaveData()
    {
        var list = new List<ItemSaveData>();

        foreach (var item in items )
        {
            list.Add(new ItemSaveData
            {
                itemId = item.itemData.itemId,
                amount = item.amount,
            });
        }
        return list;
    }

    public void LoadFromSaveData(List<ItemSaveData> data)
    {
        items.Clear();

        foreach(var save in data)
        {
            var itemData = ItemDatabase.Instance.GetItemById(save.itemId);
            if(itemData == null)
            {
                Debug.LogError("ItemData not found: " + save.itemId);
                continue;
            }
            items.Add(new InventoryItem(itemData,save.amount));

            InventoryUI ui = FindFirstObjectByType<InventoryUI>();
            if(ui != null)
            {
                ui.Refresh();
            }
        }
    }
}
