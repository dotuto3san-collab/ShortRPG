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
    public string skillName;
    public SkillType type;
    public int power;
    public SkillSlotType slotType;
}
