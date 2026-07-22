using System.Collections;
using UnityEngine;

public class SelectCommandInterrupt : IBattleCommand
{
    public IEnumerator Execute(BattleUnit user, BattleUnit target)
    {
        GameManager.Instance.ChangeState(GameState.BattleCommand);

        if(BattleCommandUI.Instance != null)
        {
            BattleCommandUI.Instance.ResetSelection();
            BattleCommandUI.Instance.Show();
        }

        BattleManager.Instance.ClearSelectedCommand();

        yield return new WaitUntil(() => BattleManager.Instance.HasSelectedCommand());

        var cmd = BattleManager.Instance.ConsumeSelectedCommand();

        yield return cmd.Execute(user, BattleManager.Instance.GetSelectedTarget());
    }
}
