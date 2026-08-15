using UnityEngine;

public enum MagicType
{
    Damage,
    Heal,
    Buff
}

public enum MagicTargetType
{
    Self,
    EnemySingle,
    EnemyAll,
}

[CreateAssetMenu(menuName = "RPG/Magic")]
public class MagicData : ScriptableObject
{
    public string magicName;

    [TextArea(2, 4)]
    public string effectText;
    [TextArea(3, 6)]
    public string description;

    public MagicType type;
    public MagicTargetType targetType;
    public int power;
}
