using UnityEngine;

[CreateAssetMenu(menuName = "RPG/BattleData")]
public class BattleData : ScriptableObject
{
    public string unitName;
    public int MaxHP;
    public int attack;
    public int defense;
    public Sprite sprite;

    [Header("ŒoŒ±’l")]
    public int expReward = 10;

    [Header("“GAI")]
    public EnemyActionData[] actionRotation;

    [Header("Šm—¦‚³‚Ú‚è")]
    [Range(0f, 1f)]
    public float idleCance = 0.10f;

    [TextArea]
    public string randomIdleText;
}
