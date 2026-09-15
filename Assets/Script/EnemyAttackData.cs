using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Enemy Action/Attack")]
public class EnemyAttackData : EnemyActionData
{
    [Header("UŒ‚")]
    public int damage;

    [Header("ƒ_ƒ[ƒW—”")]
    public int minRandomDamage = -8;
    public int maxRandomDamage = 16;
}
