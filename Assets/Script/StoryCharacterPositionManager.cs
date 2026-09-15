using UnityEngine;
using System.Collections.Generic;

public class StoryCharacterPositionManager : MonoBehaviour
{
    public static StoryCharacterPositionManager Instance { get; private set; }

    [System.Serializable]
    public class CharacterPositionEntry
    {
        public string characterId;
        public Vector3 position;
    }

    private Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RecordPosition(string characterId, Vector3 position)
    {
        if (string.IsNullOrEmpty(characterId)) return;
        positions[characterId] = position;
    }

    public bool TryGetPosition(string characterId, out Vector3 position)
    {
        return positions.TryGetValue(characterId, out position);
    }

    public List<CharacterPositionEntry> GetAllPositions()
    {
        List<CharacterPositionEntry> list = new List<CharacterPositionEntry>();
        foreach (var kv in positions)
        {
            list.Add(new CharacterPositionEntry { characterId = kv.Key, position = kv.Value });
        }
        return list;
    }

    public void LoadPositions(List<CharacterPositionEntry> entries)
    {
        positions.Clear();
        if (entries == null) return;

        foreach (var entry in entries)
        {
            if (!string.IsNullOrEmpty(entry.characterId))
            {
                positions[entry.characterId] = entry.position;
            }
        }
    }
}
