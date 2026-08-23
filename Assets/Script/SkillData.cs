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
    [Header("Šî–{î•ñ")]
    public string skillName;
    
    public SkillType type;

    [Header("Œø‰Ê")]
    public int power;

    [TextArea(2, 4)]
    public string effectText;

    [TextArea(3, 6)]
    public string descriptionText;

    [Header("‘•”õ")]
    public SkillSlotType slotType;
}
