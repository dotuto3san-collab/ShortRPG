using UnityEngine;
using System.Collections;

public class AttackCommand : IBattleCommand
{
    public IEnumerator Execute(BattleUnit user, BattleUnit target)
    {
        if (target == null) yield break;
        
        bool isAwaken = Random.value < 0.9f;

        if (isAwaken && !user.IsAwaken())
        {
            user.StartAwaken(3);

            yield return BattleLogUI.Instance.ShowLogAndWait($"{user.GetUnitName()}の魂に火が灯る...");
            yield return BattleLogUI.Instance.ShowLogAndWait($"{user.GetUnitName()}はかつての力を取り戻した！");
        }

        yield return BattleLogUI.Instance.ShowLogAndWait($"{user.data.unitName}の攻撃！");

        yield return new WaitForSeconds(0.3f);

        int baseDamage = user.GetAttack();
        int damage = Random.Range(baseDamage - 8, baseDamage + 16 + 1);

        if(damage < 0)
        {
            damage = 1;
        }

        target.TakeDamage(damage);

        yield return BattleLogUI.Instance.ShowLogAndWait(
            $"{target.data.unitName}に{damage}ダメージ与えた！"
            );
    }
}
