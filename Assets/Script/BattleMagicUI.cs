using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleMagicUI : MonoBehaviour
{
    public static BattleMagicUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject magicButtonPrefab;

    [Header("ñÇñ@å¯â ")]
    [SerializeField] private TextMeshProUGUI magicEffectText;

    List<Button> buttons = new List<Button>();

    private int lastSelectedMagicIndex = 0;

    void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Show()
    {
        root.SetActive(true);

        if (magicEffectText != null)
        {
            magicEffectText.text = "";
        }

        Refresh();
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    void Update()
    {
        if (!root.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.X) ||
           Input.GetKeyDown(KeyCode.LeftShift) ||
           Input.GetKeyDown(KeyCode.RightShift))
        {
            OnCancel();
        }
    }

    void OnCancel()
    {
        Hide();

        if (BattleCommandUI.Instance != null)
        {
            BattleCommandUI.Instance.Show();
        }
    }

    public void Refresh()
    {
        buttons.Clear();

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        GameObject firstButton = null;

        var magics = PlayerStatus.Instance.GetLearnedMagics();

        int magicIndex = 0;

        foreach (var magic in magics)
        {
            if (magic == null) continue;

            GameObject obj = Instantiate(magicButtonPrefab, content);

            var ui = obj.GetComponent<BattleMagicButtonUI>();
            ui.Setup(magic, this, magicIndex);

            var btn = obj.GetComponent<Button>();
            buttons.Add(btn);

            var nav = btn.navigation;
            nav.mode = Navigation.Mode.Explicit;
            btn.navigation = nav;

            if (firstButton == null)
            {
                firstButton = obj;
            }

            magicIndex++;
        }

        int colume = 3;
        int count = buttons.Count;

        for (int i = 0; i < count; i++)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            int row = i / colume;
            int col = i % colume;

            int rowStart = row * colume;
            int rowEnd = Mathf.Min(rowStart + colume - 1, count - 1);

            int right = i + 1;
            if (right > rowEnd) right = rowStart;

            int left = i - 1;
            if (left < rowStart) left = rowEnd;

            int down = i + colume;
            if (down >= count)
            {
                down = col;
                if (down >= count) down = i;
            }

            int up = i - colume;
            if (up < 0)
            {
                int lastRowStart = ((count - 1) / colume) * colume;
                int candidate = lastRowStart + col;

                while (candidate >= count && candidate >= col)
                {
                    candidate -= colume;
                }
                up = (candidate >= 0 && candidate < count) ? candidate : i;
            }

            nav.selectOnRight = buttons[right];
            nav.selectOnLeft = buttons[left];
            nav.selectOnDown = buttons[down];
            nav.selectOnUp = buttons[up];

            buttons[i].navigation = nav;
        }

        if (EventSystem.current != null && buttons.Count > 0)
        {
            if (lastSelectedMagicIndex < 0 ||
               lastSelectedMagicIndex >= buttons.Count)
            {
                lastSelectedMagicIndex = 0;
            }

            GameObject target = buttons[lastSelectedMagicIndex].gameObject;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target);

            var magicButton =
                target.GetComponent<BattleMagicButtonUI>();

            if (magicButton != null)
            {
                magicButton.ShowFocus();
            }
        }
    }

    public void OnMagicFocused(MagicData magic, int index)
    {
        if (magic == null)
        {
            return;
        }

        lastSelectedMagicIndex = index;

        if (magicEffectText != null)
        {
            magicEffectText.text = magic.effectText;
        }
    }

    public void OnMagicSelected(MagicData magic)
    {

        if (magic == null)
        {
            Debug.LogError("Magic is null.");
            return;
        }

        HandleMagicSelection(magic);
    }

    public bool IsActive()
    {
        return root != null && root.activeSelf;
    }

    private void HandleMagicSelection(MagicData magic)
    {
        switch (magic.targetType)
        {
            case MagicTargetType.Self:
            case MagicTargetType.EnemyAll:

                BattleManager.Instance.SetTarget(null);
                BattleManager.Instance.SetCommand(new MagicCommand(magic));

                if (BattleHelpLog.Instance != null)
                {
                    BattleHelpLog.Instance.Hide();
                }

                Hide();
                break;

            case MagicTargetType.EnemySingle:
                StartCoroutine(SelectMagicTargetFlow(magic));
                break;

            default:
                Debug.LogError("ñ¢ímÇÃTargetType" + magic.targetType);
                break;
        }
    }

    private IEnumerator SelectMagicTargetFlow(MagicData magic)
    {
        Hide();

        if (BattleHelpLog.Instance != null)
        {
            BattleHelpLog.Instance.Hide();
        }

        if(BattleTargetUI.Instance == null)
        {
            Debug.LogError("BattleMagicUI: BattleTargetUIÇ™ë∂ç›ÇµÇ‹ÇπÇÒ");
            yield break;
        }

        BattleTargetUI.Instance.SetMagic(magic);
        BattleTargetUI.Instance.Show();

        BattleLogUI.Instance.ShowImmediate("ëŒè€ÇÃìGÇëIëÇµÇƒÇ≠ÇæÇ≥Ç¢");

        yield break;
    }
}
