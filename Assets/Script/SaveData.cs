using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int money;
    public List<ItemSaveData> items = new List<ItemSaveData>();
    public Vector3 playerPosition;
    public string sceneName;
}

[Serializable]
public class ItemSaveData
{
    public string itemId;
    public int amount;
}
