using Ink.Runtime;
using UnityEngine;
using UnityEngine.Events;

public class StorySceneEvent : MonoBehaviour
{
    [System.Serializable]
    public class StoryEventEntry
    {
        [Header("イベント名")]
        public string eventName;

        [Header("実行するイベント")]
        public UnityEvent storyEvent;
    }

    [Header("このSceneで使用するストーリーイベント")]
    [SerializeField] 
    private StoryEventEntry[] eventEntries;

    private void Start()
    {
        if(StoryEventManager.Instance == null)
        {
            Debug.LogWarning(
                $"StorySceneEvent: StoryEventManager.Instanceが存在しません。" +
                $"Object = {gameObject.name}"
            );

            return;
        }

        foreach(StoryEventEntry entry in eventEntries)
        {
            if(entry == null)
            {
                continue;
            }

            StoryEventManager.Instance.RegisterEvent(
                entry.eventName,
                entry.storyEvent
            );
        }
    }

    private void OnDestroy()
    {
        if(StoryEventManager.Instance == null)
        {
            return;
        }

        foreach(StoryEventEntry entry in eventEntries)
        {
            if(entry == null)
            {
                continue;
            }

            StoryEventManager.Instance.UnregisterEvent(
                entry.eventName
            );
        }
    }
}
