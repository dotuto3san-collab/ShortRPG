using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoryEventManager : MonoBehaviour
{
    public static StoryEventManager Instance { get; private set; }

    [System.Serializable] public class StoryEventEntry
    {
        [Header("このフラグがONになったら実行")]
        public string flagName;

        [Header("実行するイベント")]
        public UnityEvent storyEvent;
    }

    [Header("ストーリーイベント")]
    [SerializeField] private List<StoryEventEntry> eventEntries =
        new List<StoryEventEntry>();

    private Dictionary<string, UnityEngine.Events.UnityEvent> events =
        new Dictionary<string, UnityEngine.Events.UnityEvent>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        RegisterEvents();
    }

    public void RegisterEvents()
    {
        events.Clear();

        foreach (StoryEventEntry entry in eventEntries)
        {
            if(entry == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(entry.flagName))
            {
                Debug.LogWarning(
                    "StoryEventManager: flagNameが設定されていないイベントがあります。");
                continue;
            }

            if (entry.storyEvent == null)
            {
                Debug.LogWarning(
                    $"StoryEventManager: UnityEventが設定されていません。 Flag = {entry.flagName}");
                continue;
            }

            events[entry.flagName] = entry.storyEvent;
        }
    }

    public void ExecuteEvent(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
        {
            return;
        }

        if(!events.TryGetValue(
            flagName,
            out UnityEvent storyEvent))
        {
            Debug.Log(
                $"StoryEventManager: イベント未登録 = {flagName}");
            return;
        }

        storyEvent.Invoke();

        Debug.Log(
            $"Story Event Execute: {flagName}");
    }
}
