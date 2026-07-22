using UnityEngine;

[CreateAssetMenu(menuName = "RPG/EncounterArea")]
public class EncounterAreaData : ScriptableObject
{
    [Header("エリアID")]
    public string areaID;

    [Header("このエリアで出現するグループ")]
    public EncounterGroupData[] encounterGroups;
}
