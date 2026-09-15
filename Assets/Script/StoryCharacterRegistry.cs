using UnityEngine;
using System.Collections.Generic;
using Ink.Runtime;

public class StoryCharacterRegistry : MonoBehaviour
{
    public static StoryCharacterRegistry Instance { get; private set; }

    private readonly Dictionary<string, StoryCharacterMover> movers
        = new Dictionary<string, StoryCharacterMover>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(string id, StoryCharacterMover mover)
    {
        if (string.IsNullOrEmpty(id) || mover == null) return;

        if (movers.ContainsKey(id))
        {
            Debug.LogWarning($"StoryCharacterRegistry: ìØÇ∂IDÇ™ä˘Ç…ìoò^Ç≥ÇÍÇƒÇ¢Ç‹Ç∑ÅBè„èëÇ´ÇµÇ‹Ç∑ÅBID = {id}");
        }

        movers[id] = mover;
    }

    public void Unregister(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        movers.Remove(id);
    }

    public StoryCharacterMover Get(string id)
    {
        movers.TryGetValue(id, out var mover);
        return mover;
    }
}
