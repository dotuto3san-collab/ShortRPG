using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillCommand : IBattleCommand
{
    private SkillData skill;
    private string skillUserName;

    public SkillCommand(
        SkillData skill,
        string skillUserName)
    {
        this.skill = skill;
        this.skillUserName = skillUserName;
    }

    public IEnumerator Execute(BattleUnit user, BattleUnit target)
    {
        if (BattleManager.Instance.AreAllEnemiesDead())
            yield break;

        if (string.IsNullOrEmpty(skillUserName))
        {
            Debug.LogError(
                "SkillCommand: スキル使用者の名前が設定されていません。");

            yield break;
        }

        yield return BattleLogUI.Instance.ShowLogAndWait(
            "スキル発動！"
        );

        yield return BattleLogUI.Instance.ShowLogAndWait(
            $"{skillUserName}は{skill.skillName}を発動した！"
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
                    if (enemy == null || enemy.IsDead())
                        continue;

                    enemy.TakeDamage(amount);

                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{enemy.GetUnitName()}に{amount}ダメージ！"
                    );
                }

                break;
            }

            case SkillType.Debuff:
            {
                int damage = skill.power;

                float attackDebuffRate =
                        skill.attackDebuffRate / 100f;

                    float defenseDebuffRate =
                            skill.defenseDebuffRate / 100f;

                var enemies = BattleManager.Instance.enemies;

                foreach(var enemy in enemies)
                {
                    if(enemy == null || enemy.IsDead())
                        continue;

                    enemy.TakeDamage(damage);

                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{enemy.GetUnitName()}に{damage}ダメージ！"
                    );

                    if (enemy.IsDead())
                        continue;

                    enemy.ApplyAttackDebuff(attackDebuffRate);
                    enemy.ApplyDefenseDebuff(defenseDebuffRate);

                    yield return BattleLogUI.Instance.ShowLogAndWait(
                        $"{enemy.GetUnitName()}の攻撃力が{skill.attackDebuffRate}%、\n" +
                        $"防御力が{skill.defenseDebuffRate}%減少した。");
                }

                break;
            }
        }

        yield return null;
    }
}
