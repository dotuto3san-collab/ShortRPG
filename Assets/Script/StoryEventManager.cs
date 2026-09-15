using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoryEventManager : MonoBehaviour
{
    public static StoryEventManager Instance { get; private set; }

    private Dictionary<string, UnityEvent> events
        = new Dictionary<string, UnityEvent>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterEvent(string flagName, UnityEvent storyEvent)
    {
        if (string.IsNullOrEmpty(flagName) || storyEvent == null) return;

        if (events.ContainsKey(flagName))
        {
            Debug.LogWarning($"StoryEventManager: ìØÇ∂flagNameÇ™ä˘Ç…ìoò^Ç≥ÇÍÇƒÇ¢Ç‹Ç∑ÅBè„èëÇ´ÇµÇ‹Ç∑ Flag = {flagName}");
        }

        events[flagName] = storyEvent;
    }

    public void UnregisterEvent(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;
        events.Remove(flagName);
    }

    public void ExecuteEvent(string flagName)
    {
        if (string.IsNullOrEmpty(flagName)) return;

        if (!events.TryGetValue(flagName, out UnityEvent storyEvent))
        {
            Debug.Log($"StoryEventManager: ÉCÉxÉìÉgñ¢ìoò^ = {flagName}");
            return;
        }

        storyEvent.Invoke();
        Debug.Log($"Story Event Execute: {flagName}");
    }
}
