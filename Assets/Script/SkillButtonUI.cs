using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SkillButtonUI : MonoBehaviour,ISelectHandler
{
    private SkillData skill;
    private SkillUI parent;

    public SkillData Skill => skill;

    public void Setup(
        SkillData skill,
        SkillUI Parent)
    {
        this.skill = skill;
        this.parent = Parent;

        if(skill == null)
        {
            return;
        }

        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(skill == null ||
           parent == null)
        {
            return;
        }

        parent.SelectSkill(skill);
    }

    private void OnClick()
    {
        if(skill == null ||
           parent == null)
        {
            return;
        }

        parent.SelectSkill(skill);
        parent.ConfirmSkill();
    }
}
