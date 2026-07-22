using UnityEngine;
using System.Collections.Generic;

public class PlayerStatus : MonoBehaviour
{
    public static PlayerStatus Instance { get; private set; }

    [Header("基本情報")]
    [SerializeField] private string playerName = "プレイヤー";

    [Header("ステータス")]
    public int maxHP = 100;
    public int currentHP;

    [SerializeField] private int baseAttack = 10;
    [SerializeField] private int baseDefense = 5;
    [SerializeField] private int baseCharge = 1;
    
    [Header("レベル")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;

    [SerializeField] private LevelTable levelTable;

    private List<MagicData> learnedMagics = new List<MagicData>();
    private List<SkillData> learnedSkills = new List<SkillData>();

    private int bonusAttack = 0;
    private int bonusDefense = 0;
    private int bonusCharge = 0;

    private Dictionary<SkillSlotType, SkillData> equippedSkills
        = new Dictionary<SkillSlotType, SkillData>();

    public System.Action OnStatusChanged;

    public int Attack => Mathf.Clamp(baseAttack + bonusAttack, 0, 255);
    public int Defense => Mathf.Clamp(baseDefense + bonusDefense, 0, 255);
    public int Charge => Mathf.Clamp(baseCharge + bonusCharge, 0, 15);
    
    public string GetPlayerName()
    {
        return playerName;
    }

    public int GetLevel() => currentLevel;
    public int GetCurrentExp() => currentExp;
    public int GetRequiredExp()
    {
        var data = levelTable.GetLevelData(currentLevel);
        return data != null ? data.requiredExp : 0;
    }

    public List<MagicData> GetLearnedMagics()
    {
        return learnedMagics;
    }
    public List<SkillData> GetLearnedSkills()
    {
        return learnedSkills;
    }

    public SkillData GetSkill(SkillSlotType slot)
    {
        if(equippedSkills.TryGetValue(slot, out var skill))
        {
            return skill;
        }
        return null;
    }

    public SkillData GetSpecialSkill()
    {
        return GetSkill(SkillSlotType.Special);
    }

    public void SetHP(int value)
    {
        currentHP = Mathf.Clamp(value, 0, maxHP);
        OnStatusChanged?.Invoke();
    }

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("PlayerStatus Awake; " + this.GetInstanceID());

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if(currentHP == 0)
        {
            currentHP = maxHP;
        }

        var data = levelTable.GetLevelData(currentLevel);
        if(data != null)
        {
            foreach(var magic in data.unlockMagics)
            {
                if(magic != null && !learnedMagics.Contains(magic))
                {
                    learnedMagics.Add(magic);
                }
            }
        }
        
        equippedSkills = new Dictionary<SkillSlotType, SkillData>();

        if(data != null)
        {
            foreach(var skill in data.unlockSkills)
            {
                if(skill != null && !learnedSkills.Contains(skill))
                {
                    learnedSkills.Add(skill);
                    EquipSkill(skill);
                }
            }
        }

        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "プレイヤー";
        }
    }

    public void SetBaseStatus(int attack,int defense,int charge)
    {
        baseAttack = attack;
        baseDefense = defense;
        baseCharge = charge;

        ClampAllStats();
        OnStatusChanged?.Invoke();
    }

    public void SetBonusStats(int atk,int def, int charge)
    {
        bonusAttack = atk;
        bonusDefense = def;
        bonusCharge = charge;

        OnStatusChanged?.Invoke();
    }

    public void ResetBonusStats()
    {
        bonusAttack = 0;
        bonusDefense = 0;
        bonusCharge = 0;

        OnStatusChanged?.Invoke();
    }

    private void ClampAllStats()
    {
        baseAttack = Mathf.Clamp(baseAttack, 0, 255);
        baseDefense = Mathf.Clamp(baseDefense, 0, 255);
        baseCharge = Mathf.Clamp(baseCharge, 0, 15);
    }

    public (int atk,int def,int chg)GetPreviewTotalStats(int bonusAtk, int bonusDef, int bonusChg)
    {
        int atk = Mathf.Clamp(baseAttack + bonusAtk, 0, 255);
        int def = Mathf.Clamp(baseDefense + bonusDef, 0, 255);
        int chg = Mathf.Clamp(baseCharge + bonusChg, 0, 15);
        return (atk, def, chg);
    }

    public int AddExperience(int amount)
    {
        if (amount <= 0) return 0;

        currentExp += amount;

        int levelUpCount = 0;

        while(true)
        {
            var data = levelTable.GetLevelData(currentLevel);
            if(data == null) break;

            if (currentExp < data.requiredExp) break;

            currentExp -= data.requiredExp;
            currentLevel++;
            levelUpCount++;

            OnLevelUp(currentLevel);
        }

        return levelUpCount;
    }

    private void OnLevelUp(int level)
    {
        var data = levelTable.GetLevelData(level);
        if(data == null) return;

        foreach(var magic in data.unlockMagics)
        {
            if(magic != null && !learnedMagics.Contains(magic))
            {
                learnedMagics.Add(magic);
                Debug.Log($"魔法取得: {magic.name}");
            }
        }

        foreach(var skill in data.unlockSkills)
        {
            if(skill != null && !learnedSkills.Contains(skill))
            {
                learnedSkills.Add(skill);
                Debug.Log($"スキル取得: {skill.name}");
                EquipSkill(skill);
            }
        }

        maxHP += 10;
        baseAttack += 2;
        baseDefense += 2;

        currentHP = maxHP;

        Debug.Log($"レベルアップ！ Lv.{level}");
    }

    private void EquipSkill(SkillData skill)
    {
        if(skill == null) return;

        var slot = skill.slotType;

        equippedSkills[slot] = skill;

        Debug.Log($"スキル装備:{slot}に{skill.name}");

        Debug.Log($"[EquipSkill] {skill.name} → slot:{slot}");
    }
}
