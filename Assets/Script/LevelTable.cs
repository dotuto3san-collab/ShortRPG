using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Level Table")]
public class LevelTable : ScriptableObject
{
    public LevelData[] levels;

    public LevelData GetLevelData(int level)
    {
        if(level <= 0 || level > levels.Length)
        {
            Debug.LogError($"Invalid level: {level}");
            return null;
        }

        return levels[level - 1];
    }
}

[System.Serializable]
public class LevelData
{
    public int level;
    public int requiredExp;

    public MagicData[] unlockMagics;
    public SkillData[] unlockSkills;
}
