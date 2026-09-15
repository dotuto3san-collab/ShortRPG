using System.Collections.Generic;
using UnityEngine;

public class StoryStateManager : MonoBehaviour
{
    public static StoryStateManager Instance { get; private set; }

    private HashSet<string> flags = new HashSet<string>();

    public List<string> GetAllFlags()
    {
        return new List<string>(flags);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    public void LoadFlags(List<string> flagList)
    {
        flags.Clear();

        if (flagList == null) return;

        foreach(var flagName in flagList)
        {
            if (!string.IsNullOrEmpty(flagName))
            {
                flags.Add(flagName);
            }
        }

        Debug.Log($"StoryStateManager: フラグを{flags.Count}件ロードしました");
    }
}
