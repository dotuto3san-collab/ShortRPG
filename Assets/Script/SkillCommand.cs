using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillCommand : IBattleCommand
{
    private SkillData skill;

    public SkillCommand(SkillData skill)
    {
        this.skill = skill;
    }

    public IEnumerator Execute(BattleUnit user, BattleUnit target)
    {
        yield return BattleLogUI.Instance.ShowLogAndWait(
            "スキル発動！"
            );

        yield return BattleLogUI.Instance.ShowLogAndWait(
            $"{user.GetUnitName()}は{skill.skillName}を発動した！"
            );

        switch (skill.type)
        {
            case SkillType.Heal:
            {
                int amount = skill.power;
                user.Heal(amount);

                yield return BattleLogUI.Instance.ShowLogAndWait(
                    $"{user.GetUnitName()}は{amount}回復した！"
                );
                break;
            }

            case SkillType.Buff:
            {
                int amount = skill.power;
                user.AddAttackBuff(amount);

                yield return BattleLogUI.Instance.ShowLogAndWait(
                    $"{user.GetUnitName()}の攻撃力が{amount}上がった!"
                    );
                break;
            }

            case SkillType.Damage:
            {
                int amount = skill.power;

                var enemies = BattleManager.Instance.enemies;

                foreach (var enemy in enemies)
                {
                    if (enemy == null || enemy.IsDead()) continue;
                    
                    enemy.TakeDamage(amount);

                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{enemy.GetUnitName()}に{amount}ダメージ！"
                    );
                }
                break;
            }
        }

        yield return null;
    }
}
