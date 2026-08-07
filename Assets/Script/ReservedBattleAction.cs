using UnityEngine;

public class ReservedBattleAction
{
    public IBattleCommand Command { get; }
    public BattleUnit Target { get; }

    public ReservedBattleAction(
        IBattleCommand command,
        BattleUnit target)
    {
        Command = command;
        Target = target;
    }
}
