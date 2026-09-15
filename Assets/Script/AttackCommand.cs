using UnityEngine;
using System.Collections;

public class AttackCommand : IBattleCommand
{
    public IEnumerator Execute(BattleUnit user, BattleUnit target)
    {
        BattleUnit resolvedTarget =
            BattleManager.Instance.ResolveAttackTarget(target);

        if (resolvedTarget == null) yield break;
        
        bool isAwaken = Random.value < 0.05f;

        if (isAwaken && !user.IsAwaken())
        {
            user.StartAwaken(3);

            yield return BattleLogUI.Instance.ShowLogAndWait($"{user.GetUnitName()}の魂に火が灯る...");
            yield return BattleLogUI.Instance.ShowLogAndWait($"{user.GetUnitName()}はかつての力を取り戻した！");
        }

        yield return BattleLogUI.Instance.ShowLogAndWait($"{user.GetUnitName()}の攻撃！");

        yield return new WaitForSeconds(0.3f);

        int baseDamage = user.GetAttack();
        int damage = Random.Range(baseDamage - 8, baseDamage + 16 + 1);

        if(damage < 0)
        {
            damage = 1;
        }

        resolvedTarget.TakeDamage(damage);

        yield return BattleLogUI.Instance.ShowLogAndWait(
            $"{resolvedTarget.GetBattleDisplayName()}に{damage}ダメージ与えた！"
            );
    }
}
