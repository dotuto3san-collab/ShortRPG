using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    public static AbilityUI Instance { get; private set; }

    public enum AbilityState
    {
        SelectAbility,
        Magic,
        Skill
    }

    [Header("画面")]
    [SerializeField] private GameObject abilityPanel;

    [Header("魔法・スキル選択")]
    [SerializeField] private GameObject magicButton;
    [SerializeField] private GameObject skillButton;

    [Header("確認画面")]
    [SerializeField] private GameObject magicPanel;

    [Header("魔法一覧")]
    [SerializeField] private Transform magicContent;
    [SerializeField] private GameObject magicButtonPrefab;

    [Header("魔法説明")]
    [SerializeField] private TextMeshProUGUI magicEffectText;
    [SerializeField] private TextMeshProUGUI magicDescriptionText;

    [Header("最初に選択するUI")]
    [SerializeField] private GameObject firstSelectedObject;

    [Header("魔法使用不可メッセージ")]
    [SerializeField] private GameObject magicCannotUseMessage;

    private AbilityState currentState;
    private Coroutine magicMessageCoroutine;

    public AbilityState CurrentState => currentState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CloseAllPanels();
    }

    public void Open()
    {
        if(SkillUI.Instance != null)
        {
            SkillUI.Instance.Close();
        }

        if(abilityPanel != null)
        {
            abilityPanel.SetActive(true);
        }

        if(magicPanel != null)
        {
            magicPanel.SetActive(false);
        }

        if(magicCannotUseMessage != null)
        {
            magicCannotUseMessage.SetActive(false);
        }

        currentState = AbilityState.SelectAbility;

        SelectFirstObject();
    }

    public void OpenMagic()
    {
        currentState = AbilityState.Magic;

        if(magicPanel != null)
        {
            magicPanel.SetActive(true);
        }

        if(magicCannotUseMessage != null)
        {
            magicCannotUseMessage.SetActive(false);
        }

        RefreshMagicList();
    }

    public void OpenSkill()
    {
        currentState = AbilityState.Skill;

        if(magicPanel != null)
        {
            magicPanel.SetActive(false);
        }

        if(magicCannotUseMessage != null)
        {
            magicCannotUseMessage.SetActive(false);
        }

        if(SkillUI.Instance != null)
        {
            SkillUI.Instance.Open();
        }
    }

    private void RefreshMagicList()
    {
        if(magicPanel == null)
        {
            Debug.LogError("magicPanelが設定されていません");
            return;
        }

        if(PlayerStatus.Instance == null)
        {
            Debug.LogError("PlayerStatus Instance not found");
            return;
        }

        foreach(Transform child in magicContent)
        {
            Destroy(child.gameObject);
        }

        List<MagicData> magics = PlayerStatus.Instance.GetLearnedMagics();

        List<Button> buttons = new List<Button>();

        GameObject firstButton = null;

        foreach(var magic in magics)
        {
            if (magic == null) continue;

            GameObject obj = Instantiate(magicButtonPrefab, magicContent);

            if(firstButton == null)
            {
                firstButton = obj;
            }

            AbilityMagicButtonUI buttonUI =
                obj.GetComponent<AbilityMagicButtonUI>();

            if(buttonUI != null)
            {
                buttonUI.Setup(magic);
            }

            Button button = obj.GetComponent<Button>();

            if(button != null)
            {
                buttons.Add(button);
            }
        }

        int columnCount = 3;
        int count = buttons.Count;

        for(int i = 0; i < count; i++)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            int row = i / columnCount;
            int col = i % columnCount;

            int rowStart = row * columnCount;
            int rowEnd = Mathf.Min(rowStart + columnCount - 1, count - 1);

            int right = i + 1;

            if(right > rowEnd)
            {
                right = rowStart;
            }

            int left = i - 1;

            if(left < rowStart)
            {
                left = rowEnd;
            }

            int down = i + columnCount;

            if(down >= count)
            {
                down = col;

                if(down >= count)
                {
                    down = i;
                }
            }

            int up = i - columnCount;

            if(up < 0)
            {
                int lastRowStart =
                    ((count - 1) / columnCount) * columnCount;

                int candidate = lastRowStart + col;

                while(candidate >= count && candidate >= col)
                {
                    candidate -= columnCount;
                }

                up = (candidate >= 0 && candidate < count)
                     ? candidate
                     : i;
            }

            nav.selectOnRight = buttons[right];
            nav.selectOnLeft = buttons[left];
            nav.selectOnUp = buttons[up];
            nav.selectOnDown = buttons[down];

            buttons[i].navigation = nav;
        }

        if(firstButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }

        Debug.Log($"AbilityUI Magic Refresh: {magics.Count} magics");
    }

    public void Confirm()
    {
        switch (currentState)
        {
            case AbilityState.SelectAbility:
                ConfirmAbilitySelection();
                break;

            case AbilityState.Magic:
                ShowMagicCannotUseMessage();
                break;

            case AbilityState.Skill:
                if (SkillUI.Instance != null)
                {
                    SkillUI.Instance.Open();
                }
                break;
        }
    }

    private void ConfirmAbilitySelection()
    {
        if(EventSystem.current == null)
        {
            return;
        }

        GameObject selected =
            EventSystem.current.currentSelectedGameObject;

        if(selected == null)
        {
            return;
        }

        if (selected == magicButton)
        {
            OpenMagic();
        }
        else if (selected == skillButton)
        {
            OpenSkill();
        }
    }

    public void OnMagicSelected(MagicData magic)
    {
        if(magic == null)
        {
            Debug.LogError("Magic is null");
            return;
        }

        ShowMagicCannotUseMessage();
    }

    public void OnMagicFocused(MagicData magic)
    {
        if(magic == null)
        {
            return;
        }

        if(magicEffectText != null)
        {
            magicEffectText.text = magic.effectText;
        }

        if(magicDescriptionText != null)
        {
            magicDescriptionText.text = magic.description;
        }
    }

    private void ShowMagicCannotUseMessage()
    {
        if (magicCannotUseMessage == null)
        {
            return;
        }

        if (magicMessageCoroutine != null)
        {
            StopCoroutine(magicMessageCoroutine);
            magicMessageCoroutine = null;
        }

        magicCannotUseMessage.SetActive(true);

        magicMessageCoroutine =
            StartCoroutine(HideMagicCannotUseMessageAfterDelay());
    }

    private IEnumerator HideMagicCannotUseMessageAfterDelay()
    {
        yield return new WaitForSeconds(1.0f);

        if(magicCannotUseMessage != null)
        {
            magicCannotUseMessage.SetActive(false);
        }

        magicMessageCoroutine = null;
    }

    private void SelectFirstObject()
    {
        if(EventSystem.current == null)
        {
            return;
        }

        GameObject target = firstSelectedObject;

        if(target == null)
        {
            target = magicButton;
        }

        if(target == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }

    private void SelectObject(GameObject target)
    {
        if(EventSystem.current == null)
        {
            return;
        }

        if(target == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }

    public void FocusSkillButton()
    {
        currentState = AbilityState.SelectAbility;

        SelectObject(skillButton);
    }

    public void HandleCancel()
    {
        switch (currentState)
        {
            case AbilityState.Magic:
                currentState = AbilityState.SelectAbility;
                
                if (magicPanel != null)
                {
                    magicPanel.SetActive(false);
                }

                if(magicCannotUseMessage != null)
                {
                    magicCannotUseMessage.SetActive(false);
                }

                SelectObject(magicButton);
                break;

            case AbilityState.Skill:
                currentState = AbilityState.SelectAbility;

                if(SkillUI.Instance != null)
                {
                    SkillUI.Instance.Close();
                }

                SelectObject(skillButton);
                break;

            case AbilityState.SelectAbility:

                if(MenuManager.Instance != null)
                {
                    MenuManager.Instance.SetMenuState(MenuState.Main);
                }
                break;
        }
    }

    private void CloseAllPanels()
    {
        if(abilityPanel != null)
        {
            abilityPanel.SetActive(false);
        }

        if(magicPanel != null)
        {
            magicPanel.SetActive(false);
        }

        if(magicCannotUseMessage != null)
        {
            magicCannotUseMessage.SetActive(false);
        }
    }
}
