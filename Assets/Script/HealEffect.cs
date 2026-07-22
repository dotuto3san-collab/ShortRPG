using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "RPG/ItemEffect/HealEffect")]
public class HealEffect : ItemEffect
{
    public int healAmount;

    public override IEnumerator Apply(BattleUnit user, BattleUnit target)
    {
        if (target == null)
        {
            if(PlayerStatus.Instance == null)
            {
                Debug.LogError("PlayerStatus.Instance が存在しません");
                yield break;
            }

            PlayerStatus.Instance.currentHP += healAmount;

            if (PlayerStatus.Instance.currentHP > PlayerStatus.Instance.maxHP)
            {
                PlayerStatus.Instance.currentHP = PlayerStatus.Instance.maxHP;
            }

            Debug.Log($"プレイヤーは{healAmount}回復した！");

            yield break;
        }

        target.Heal(healAmount);

        yield return BattleLogUI.Instance
            .ShowLogAndWait($"{target.data.unitName}は{healAmount}回復した！");
    }
}
