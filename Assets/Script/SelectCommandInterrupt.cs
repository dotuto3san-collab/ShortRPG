using System.Collections;
using UnityEngine;

public class SelectCommandInterrupt : IBattleCommand
{
    public IEnumerator Execute(BattleUnit user, BattleUnit target)
    {
        yield return BattleLogUI.Instance.ShowLogAndWait("追加コマンド発動！");

        if (BattleManager.Instance.AreAllEnemiesDead())
            yield break;

        GameManager.Instance.ChangeState(GameState.BattleCommand);

        if(BattleCommandUI.Instance != null)
        {
            BattleCommandUI.Instance.ResetSelection();
            BattleCommandUI.Instance.Show();
        }

        BattleManager.Instance.ClearSelectedCommand();

        yield return new WaitUntil(() => 
            BattleManager.Instance.HasSelectedCommand() ||
            BattleManager.Instance.AreAllEnemiesDead());

        if(BattleManager.Instance.AreAllEnemiesDead())
            yield break;

        IBattleCommand command = BattleManager.Instance.ConsumeSelectedCommand();

        BattleUnit selectedTarget = BattleManager.Instance.GetSelectedTarget();

        GameManager.Instance.ChangeState(GameState.BattleExecute);

        yield return command.Execute(user, selectedTarget);
    }
}
