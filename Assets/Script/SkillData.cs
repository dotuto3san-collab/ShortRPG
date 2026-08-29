using UnityEngine;

public enum SkillType
{
    Damage,
    Heal,
    Debuff,
    Buff,
    Utility,
    Special
}

[CreateAssetMenu(menuName = "RPG/Skill")]
public class SkillData : ScriptableObject
{
    [Header("基本情報")]
    public string skillName;
    
    public SkillType type;

    [Header("効果")]
    public int power;

    [Header("デバフ効果")]
    [Range(0, 100)]
    public int attackDebuffRate;

    [Range(0, 100)]
    public int defenseDebuffRate;

    [TextArea(2, 4)]
    public string effectText;

    [TextArea(3, 6)]
    public string descriptionText;

    [Header("装備")]
    public SkillSlotType slotType;
}
