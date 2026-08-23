using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CompanionManager : MonoBehaviour
{
    public static CompanionManager Instance { get; private set; }

    [Header("主人公")]
    [SerializeField] CompanionData playerData;

    private List<CompanionStatus> companions
        = new List<CompanionStatus>();

    public IReadOnlyList<CompanionStatus> Companions => companions;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AddPlayer();
    }

    private void AddPlayer()
    {
        if (playerData == null)
        {
            Debug.LogError(
                "CompanionManager: playerDataが設定されていません。");

            return;
        }

        CompanionStatus status = new CompanionStatus(playerData);
        status.Initialize();

        companions.Insert(0, status);

        Debug.Log($"主人公をスキル一覧に登録: {playerData.companionName}");
    }

    public void AddCompanion(CompanionData data)
    {
        if(data == null)
        {
            Debug.LogWarning("CompanionManager: CompanionDataがnullです。");
            return;
        }

        if (HasCompanion(data))
        {
            return;
        }

        CompanionStatus status = new CompanionStatus(data);
        status.Initialize();

        companions.Add(status);

        Debug.Log($"仲間加入: {data.companionName}");
    }

    public void RemoveCompanion(CompanionData data)
    {
        if(data == null)
        {
            return;
        }

        CompanionStatus companion = GetCompanion(data);

        if(companion == null)
        {
            return;
        }

        companions.Remove(companion);

        Debug.Log($"仲間離脱: {data.companionName}");
    }

    public bool HasCompanion(CompanionData data)
    {
        return GetCompanion(data) != null;
    }

    public CompanionStatus GetCompanion(CompanionData data)
    {
        if(data == null)
        {
            return null;
        }

        foreach(var companion in companions)
        {
            if(companion != null && companion.Data == data)
            {
                return companion;
            }
        }

        return null;
    }

    public CompanionStatus GetCompanion(int index)
    {
        if(index < 0 || index >= companions.Count)
        {
            return null;
        }

        return companions[index];
    }

    public CompanionStatus GetPlayerStatus()
    {
        if(companions.Count == 0)
        {
            return null;
        }

        return companions[0];
    }

    public IReadOnlyList<CompanionStatus> GetCompanion()
    {
        return companions;
    }

    public void EquipSkill(CompanionStatus companion, SkillData skill)
    {
        if(companion == null || skill == null)
        {
            return;
        }

        companion.EquipSkill(skill);

        if(companion == GetPlayerStatus())
        {
            if(PlayerStatus.Instance != null)
            {
                PlayerStatus.Instance.SetEquippedSkill(skill);

                Debug.Log(
                    $"[CompanionManager] 主人公のスペシャルスキルを同期: " +
                    $"{skill.skillName}");
            }
            else
            {
                Debug.LogWarning(
                    "[CompanionManager] PlayerStatus.Instanceが存在しません");
            }
        }

        Debug.Log(
            "仲間スキル装備: " + 
            $"{companion.Data.companionName} / {skill.skillName}");
    }
}
