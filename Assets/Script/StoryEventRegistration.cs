using UnityEngine;
using UnityEngine.Events;

public class StoryEventRegistration : MonoBehaviour
{
    [Header("イベント設定")]
    [SerializeField] private string eventName;

    [SerializeField] private UnityEvent storyEvent;

    private void Start()
    {
        if(StoryEventManager.Instance == null)
        {
            Debug.LogWarning(
                $"StoryEventRegistration: " +
                $"StoryEventManager.Instanceが存在しません。" +
                $"Object = {gameObject.name}");

            return;
        }

        StoryEventManager.Instance.RegisterEvent(
            eventName,
            storyEvent
        );
    }

    private void OnDestroy()
    {
        if(StoryEventManager.Instance == null)
        {
            return;
        }

        StoryEventManager.Instance.UnregisterEvent(
            eventName
        );
    }
}
