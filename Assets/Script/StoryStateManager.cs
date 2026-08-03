using System.Collections.Generic;
using UnityEngine;

public class StoryStateManager : MonoBehaviour
{
    public static StoryStateManager Instance { get; private set; }

    private HashSet<string> flags = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void SetFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
        {
            Debug.LogWarning("StoryStateManager.SetFlag: flagName is empty");
            return;
        }

        if (flags.Contains(flagName))
        {
            return;
        }

        flags.Add(flagName);

        Debug.Log($"Story Flag ON: {flagName}");

        if(StoryEventManager.Instance != null)
        {
            StoryEventManager.Instance.ExecuteEvent(flagName);
        }
    }

    public bool HasFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
        {
            return false;
        }

        return flags.Contains(flagName);
    }

    public void ClearFlag(string flagName)
    {
        if (string.IsNullOrEmpty(flagName))
        {
            Debug.LogWarning("StoryStateManager.ClearFlag: flagName is empty");
            return;
        }

        flags.Remove(flagName);

        Debug.Log($"Story Flag OFF: {flagName}");
    }
}
