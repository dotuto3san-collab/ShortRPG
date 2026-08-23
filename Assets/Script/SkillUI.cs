using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SkillUI : MonoBehaviour
{
    public static SkillUI Instance { get; private set; }

    private enum SkillState
    {
        CompanionSelect,
        SkillSelect
    }

    [Header("ルート")]
    [SerializeField] private GameObject root;

    [Header("仲間一覧")]
    [SerializeField] private Transform companionContent;
    [SerializeField] private GameObject companionButtonPrefab;

    [Header("スキル一覧")]
    [SerializeField] private Transform skillContent;
    [SerializeField] private GameObject skillButtonPrefab;

    [Header("装備表示")]
    [SerializeField] private GameObject equippedArea;

    [Header("スキル情報")]
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillEffectText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;

    private readonly List<Button> companionButtons
        = new List<Button>();

    private readonly List<Button> skillButtons
        = new List<Button>();

    private SkillState currentState;

    private int selectedCompanionIndex;
    private int selectedSkillIndex;

    private CompanionStatus selectedCompanion;
    private SkillData selectedSkill;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if(root != null)
        {
            root.SetActive(false);
        }
    }

    public void Open()
    {
        if(root != null)
        {
            root.SetActive(true);
        }

        selectedCompanion = null;
        selectedSkill = null;

        selectedCompanionIndex = 0;
        selectedSkillIndex = 0;

        ClearSkillInfo();

        currentState = SkillState.CompanionSelect;

        HideSkillContent();
        HideEquippedArea();

        RefreshCompanions();
    }

    public void Close()
    {
        if(root != null)
        {
            root.SetActive(false);
        }
    }

    private void RefreshCompanions()
    {
        companionButtons.Clear();

        foreach(Transform child in companionContent)
        {
            Destroy(child.gameObject);
        }

        if(CompanionManager.Instance == null)
        {
            Debug.LogError("SkillUI: CompanionManager.Instanceが存在しません");
            return;
        }

        var companions = CompanionManager.Instance.GetCompanion();

        for(int i = 0; i < companions.Count; i++)
        {
            CompanionStatus companion = companions[i];

            if(companion == null || companion.Data == null)
            {
                continue;
            }

            GameObject obj =
                Instantiate(companionButtonPrefab, companionContent);

            SkillCompanionButtonUI buttonUI =
                obj.GetComponent<SkillCompanionButtonUI>();

            if (buttonUI == null)
            {
                Debug.LogError(
                    $"SkillUI.RefreshCompanions: " +
                    $"生成したPrefab「{obj.name}」に" +
                    $"SkillCompanionButtonUIがありません。");

                continue;
            }

            buttonUI.Setup(companion, this);

            Button button = obj.GetComponent<Button>();

            if (button == null)
            {
                Debug.LogError(
                    $"SkillUI.RefreshCompanions: " +
                    $"生成したPrefab「{obj.name}」にButtonがありません。");

                continue;
            }

            companionButtons.Add(button);
        }

        if(companionButtons.Count == 0)
        {
            ClearSkillInfo();
            return;
        }

        SetupHorizontalNavigation(companionButtons);

        selectedCompanionIndex =
            Mathf.Clamp(
                selectedCompanionIndex,
                0,
                companionButtons.Count - 1);

        SelectButton(companionButtons[selectedCompanionIndex].gameObject);
    }

    public void SelectCompanion(CompanionStatus companion)
    {
        if(companion == null)
        {
            return;
        }

        selectedCompanion = companion;

        for(int i = 0; i < companionButtons.Count; i++)
        {
            SkillCompanionButtonUI ui =
                companionButtons[i].GetComponent<SkillCompanionButtonUI>();

            if(ui != null && ui.Companion == companion)
            {
                selectedCompanionIndex = i;
                break;
            }
        }
    }

    public void ConfirmCompanion()
    {
        if(selectedCompanion == null)
        {
            if(companionButtons.Count == 0)
            {
                return;
            }

            selectedCompanion =
                companionButtons[selectedCompanionIndex]
                    .GetComponent<SkillCompanionButtonUI>()
                    .Companion;
        }

        currentState = SkillState.SkillSelect;

        ShowSkillContent();
        RefreshSkills();
    }

    private void RefreshSkills()
    {
        skillButtons.Clear();

        foreach(Transform child in skillContent)
        {
            Destroy(child.gameObject);
        }

        ClearSkillInfo();

        if(selectedCompanion == null)
        {
            return;
        }

        var skills = selectedCompanion.LearnedSkills;

        for(int i = 0;i < skills.Count; i++)
        {
            SkillData skill = skills[i];

            if(skill == null)
            {
                continue;
            }

            GameObject obj =
                Instantiate(skillButtonPrefab, skillContent);

            SkillButtonUI buttonUI =
                obj.GetComponent<SkillButtonUI>();

            if(buttonUI != null)
            {
                buttonUI.Setup(skill, this);
            }

            Button button = obj.GetComponent<Button>();

            if(button != null)
            {
                skillButtons.Add(button);
            }
        }

        if(skillButtons.Count == 0)
        {
            HideEquippedArea();
            return;
        }

        SetupHorizontalNavigation(skillButtons);

        selectedSkillIndex =
            Mathf.Clamp(
                selectedSkillIndex,
                0,
                skillButtons.Count - 1);

        SelectButton(skillButtons[selectedSkillIndex].gameObject);

        SkillButtonUI selectedUI =
            skillButtons[selectedSkillIndex]
                .GetComponent<SkillButtonUI>();

        if(selectedUI != null)
        {
            SelectSkill(selectedUI.Skill);
        }
    }

    public void SelectSkill(SkillData skill)
    {
        if(skill == null)
        {
            return;
        }

        selectedSkill = skill;

        for(int i = 0; i < skillButtons.Count; i++)
        {
            SkillButtonUI ui =
                skillButtons[i].GetComponent < SkillButtonUI>();

            if(ui != null && ui.Skill == skill)
            {
                selectedSkillIndex = i;
                break;
            }
        }

        if(skillNameText != null)
        {
            skillNameText.text = skill.skillName;
        }

        if(skillEffectText != null)
        {
            skillEffectText.text = skill.effectText;
        }

        if(skillDescriptionText != null)
        {
            skillDescriptionText.text = skill.descriptionText;
        }

        UpdateEquippedArea();
    }

    public void ConfirmSkill()
    {
        if(selectedCompanion == null ||
           selectedSkill == null)
        {
            return;
        }

        CompanionManager.Instance.EquipSkill(
            selectedCompanion,
            selectedSkill);

        RefreshSkills();
    }

    public void HandleCancel()
    {
        if(currentState == SkillState.SkillSelect)
        {
            currentState = SkillState.CompanionSelect;

            selectedSkill = null;

            ClearSkillInfo();

            HideSkillContent();
            HideEquippedArea();

            RefreshCompanions();
            return;
        }

        Close();

        if(AbilityUI.Instance != null)
        {
            AbilityUI.Instance.FocusSkillButton();
        }
    }

    private void SetupHorizontalNavigation(List<Button> buttons)
    {
        int count = buttons.Count;

        if(count == 0)
        {
            return;
        }

        for(int i = 0; i < count; i++)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            int right = (i + 1) % count;
            int left = (i - 1 + count) % count;

            nav.selectOnRight = buttons[right];
            nav.selectOnLeft = buttons[left];

            buttons[i].navigation = nav;
        }
    }

    private void SelectButton(GameObject target)
    {
        if(EventSystem.current == null ||
           target == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }

    private void ClearSkillInfo()
    {
        if(skillNameText != null)
        {
            skillNameText.text = "";
        }

        if(skillEffectText != null)
        {
            skillEffectText.text = "";
        }

        if(skillDescriptionText != null)
        {
            skillDescriptionText.text = "";
        }
    }

    private void ShowSkillContent()
    {
        if (skillContent == null)
        {
            return;
        }

        skillContent.gameObject.SetActive(true);
    }

    private void HideSkillContent()
    {
        if(skillContent == null)
        {
            return;
        }

        skillContent.gameObject.SetActive(false);
    }

    private void UpdateEquippedArea()
    {
        if(equippedArea == null)
        {
            return;
        }

        bool visible =
            selectedSkill != null &&
            IsSkillEquipped(selectedSkill);

        equippedArea.SetActive(visible);
    }

    private void HideEquippedArea()
    {
        if (equippedArea == null)
        {
            return;
        }

        equippedArea.SetActive(false);
    }

    public bool IsSkillEquipped(SkillData skill)
    {
        if(selectedCompanion == null ||
           skill == null)
        {
            return false;
        }

        return selectedCompanion.IsSkillEquipped(skill);
    }

    public bool IsActive()
    {
        return root != null && root.activeSelf;
    }
}
