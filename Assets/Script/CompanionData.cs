using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Companion")]
public class CompanionData : ScriptableObject
{
    [Header("基本情報")]
    public string companionName;

    [Header("表示")]
    public Sprite icon;

    [Header("初期習得スキル")]
    public SkillData[] initialSkills;
}
