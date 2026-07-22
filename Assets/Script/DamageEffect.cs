using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "RPG/ItemEffect/DamageEffect")]
public class DamageEffect : ItemEffect
{
    [Header("ダメージ量")]
    public int damage;

    public override IEnumerator Apply(BattleUnit user, BattleUnit target)
    {
        if(targetType == TargetType.AllEnemies)
        {
            foreach (var enemy in BattleManager.Instance.enemies)
            {
                if(enemy.IsDead()) continue;

                enemy.TakeDamage(damage);

                yield return BattleLogUI.Instance.ShowLogAndWait(
                    $"{enemy.data.unitName}に{damage}ダメージ与えた！"
                );
            }
        }
        else
        {
            if(target == null)
            {
                Debug.LogError("Damage target is null");
                yield break;
            }

            target.TakeDamage(damage);

            yield return BattleLogUI.Instance.ShowLogAndWait(
                $"{target.data.unitName}に{damage}ダメージ与えた！"
            );
        }
    }
}
