using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Enemy Action/Idle")]
public class EnemyIdleData : EnemyActionData
{
    [Header("‚³‚Ú‚è")]
    [TextArea]
    public string idleText;
}
