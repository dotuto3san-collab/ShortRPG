using UnityEngine;

[CreateAssetMenu(menuName = "RPG/BattleData")]
public class BattleData : ScriptableObject
{
    public string unitName;
    public int MaxHP;
    public int attack;
    public Sprite sprite;

    [Header("åoå±íl")]
    public int expReward = 10;
}
