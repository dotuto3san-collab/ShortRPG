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
    public List<string> storyFlags = new List<string>();

    public List<StoryCharacterPositionManager.CharacterPositionEntry> characterPositions
        = new List<StoryCharacterPositionManager.CharacterPositionEntry>();
}

[Serializable]
public class ItemSaveData
{
    public string itemId;
    public int amount;
}
