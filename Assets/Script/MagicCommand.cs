using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicCommand : IBattleCommand
{
    private MagicData magic;

    public MagicCommand(MagicData magic)
    {
        this.magic = magic;
    }

    public IEnumerator Execute(BattleUnit user, BattleUnit target)
    {
        yield return BattleLogUI.Instance.ShowLogAndWait(
            $"{user.GetUnitName()}は{magic.magicName}を唱えた！"
        );

        switch (magic.type)
        {
            case MagicType.Damage:
            {
                if(magic.targetType == MagicTargetType.EnemyAll)
                {
                    foreach(var enemy in BattleManager.Instance.enemies)
                    {
                        if (!enemy.IsDead())
                        {
                            enemy.TakeDamage(magic.power);

                            yield return BattleLogUI.Instance.ShowLogAndWait(
                                $"{enemy.GetUnitName()}に{magic.power}ダメージ"
                            );
                        }
                    }
                }
                else if(target != null)
                {
                    target.TakeDamage(magic.power);

                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{target.GetUnitName()}に{magic.power}ダメージ！"
                    );
                }
                break;
            }

            case MagicType.Heal:
            {
                if(magic.targetType == MagicTargetType.Self)
                {
                    user.Heal(magic.power);

                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{user.GetUnitName()}のHPが{magic.power}回復した！"
                    );
                }
                break;
            }

            case MagicType.Buff:
            {
                user.AddAttackBuff(magic.power);

                yield return BattleLogUI.Instance.ShowLogAndWait(
                    $"{user.GetUnitName()}の攻撃力が{magic.power}上昇！"
                );
                break;
            }
        }

        yield return null;
    }
}
