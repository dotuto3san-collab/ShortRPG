using UnityEngine;
using System.Collections;

public interface IBattleCommand
{
    IEnumerator Execute(BattleUnit user, BattleUnit target);
}
