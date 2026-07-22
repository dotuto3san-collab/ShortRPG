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
    public MagicType type;
    public MagicTargetType targetType;
    public int power;
}
