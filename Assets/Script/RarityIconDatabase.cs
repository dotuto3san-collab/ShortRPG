using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu (fileName = "RarityIconDatabase", menuName = "RPG/RarityIconDatabase")]
public class RarityIconDatabase : ScriptableObject
{
    [System.Serializable]
    public class RarityIconPair
    {
        public ItemData.Rarity rarity;
        public Sprite icon;
    }

    [SerializeField] private List<RarityIconPair> rarityIcons;

    private Dictionary<ItemData.Rarity, Sprite> cache;

    private void OnEnable()
    {
        cache = new Dictionary<ItemData.Rarity, Sprite>();
        
        if(rarityIcons == null)
        {
            rarityIcons = new List<RarityIconPair>();
            return;
        }

        foreach (var pair in rarityIcons)
        {
            if (!cache.ContainsKey(pair.rarity))
            {
                cache.Add(pair.rarity, pair.icon);
            }
        }
    }

    public Sprite GetIcon(ItemData.Rarity rarity)
    {
        if(cache != null && cache.TryGetValue(rarity, out var icon))
        {
            return icon;
        }

        Debug.LogWarning($"RarityIcon not found: {rarity}");
        return null;
    }
}
