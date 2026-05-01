using UnityEngine;
using System.Collections.Generic;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;

    private Dictionary<string, ItemData> map;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        map = new Dictionary<string, ItemData>();

        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");

        foreach (var item in allItems)
        {
            if (string.IsNullOrEmpty(item.itemId))
            {
                Debug.LogError($"ItemID is empty: {item.name}");
                continue;
            }

            if(map.ContainsKey(item.itemId))
            {
                Debug.LogError($"Duplicate ItemData: {item.itemId}");
                continue;
            }

            map[item.itemId] = item;
        }

        Debug.Log($"ItemDatabase loaded: {map.Count} items");
    }

    public ItemData GetItemById(string id)
    {
        if(map.TryGetValue(id, out var item))
            return item;

        return null;
    }
}
