using UnityEngine;

[System.Serializable]
public class EquipData
{
    [Header("装備ステータス")]
    public int attackPower;
    public int defensePower;
    public int chargePower;

    [Header("装備種別")]
    public EquipType equipType;

    public enum EquipType
    {
        None,
        Weapon,
        Armor,
        Accessory
    }
}
