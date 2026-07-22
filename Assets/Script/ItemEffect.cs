using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewItemEffect", menuName = "RPG/ItemEffect")]
public abstract class ItemEffect : ScriptableObject
{
    public TargetType targetType;

    public enum TargetType
    {
        Self,
        Enemy,
        AllEnemies
    }

    public enum EffectType
    {
        Heal,
        Damage,
    }

    public abstract IEnumerator Apply(BattleUnit user, BattleUnit target);
}
