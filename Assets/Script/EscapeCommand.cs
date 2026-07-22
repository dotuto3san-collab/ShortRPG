using UnityEngine;
using System.Collections;

public class EscapeCommand : IBattleCommand
{
    public IEnumerator Execute(BattleUnit user,BattleUnit target)
    {
        yield return BattleLogUI.Instance.ShowLogAndWait("í“¬‚©‚ç“¦‚°o‚µ‚½I");

        BattleManager.Instance.RequestEscape();
    }
}
