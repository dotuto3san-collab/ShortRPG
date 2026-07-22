using UnityEngine;

public class SkillBarUI : MonoBehaviour
{
    public static SkillBarUI Instance;

    [SerializeField] private SkillBarUnitUI[] units;

    void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        foreach(var u in units)
        {
            u.Init();
        }
    }

    public void UseSkill(int index)
    {
        if (index < 0 || index >= units.Length) return;

        units[index].Use();
    }

    public void ResetAll()
    {
        Init();
    }
}
