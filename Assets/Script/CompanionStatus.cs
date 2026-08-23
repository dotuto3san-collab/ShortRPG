using System.Collections.Generic;

public class CompanionStatus
{
    public CompanionData Data { get; }

    private List<SkillData> learnedSkills
        = new List<SkillData>();

    private SkillData equippedSkill;

    public IReadOnlyList<SkillData> LearnedSkills => learnedSkills;

    public SkillData EquippedSkill => equippedSkill;

    public CompanionStatus(CompanionData data)
    {
        Data = data;
    }

    public void Initialize()
    {
        learnedSkills.Clear();
        equippedSkill = null;

        if(Data == null)
        {
            return;
        }

        if(Data.initialSkills == null)
        {
            return;
        }

        foreach(var skill in Data.initialSkills)
        {
            if(skill == null)
            {
                continue;
            }

            if (!learnedSkills.Contains(skill))
            {
                learnedSkills.Add(skill);
            }
        }

        if(learnedSkills.Count > 0)
        {
            equippedSkill = learnedSkills[0];
        }
    }

    public void LearnSkill(SkillData skill)
    {
        if(skill == null)
        {
            return;
        }

        if (learnedSkills.Contains(skill))
        {
            return;
        }

        learnedSkills.Add(skill);
        equippedSkill = skill;
    }

    public void EquipSkill(SkillData skill)
    {
        if(skill == null)
        {
            return;
        }

        if (!learnedSkills.Contains(skill))
        {
            return;
        }

        equippedSkill = skill;
    }

    public bool HasSkill(SkillData skill)
    {
        return skill != null && learnedSkills.Contains(skill);
    }

    public bool IsSkillEquipped(SkillData skill)
    {
        return skill != null && equippedSkill == skill;
    }
}
