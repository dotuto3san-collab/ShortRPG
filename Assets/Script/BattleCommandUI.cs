using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class BattleCommandUI : MonoBehaviour
{
    public static BattleCommandUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button sorceryButton;
    [SerializeField] private Button itemButton;
    [SerializeField] private Button escapeButton;

    private GameObject lastSelectedButton;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        root.SetActive(false);

        attackButton.onClick.AddListener(OnAttack);
        sorceryButton.onClick.AddListener(OnMagic);
        itemButton.onClick.AddListener(OnItem);
        escapeButton.onClick.AddListener(OnEscape);

        SetupNavigation();
    }

    void Update()
    {
        if(!IsVisible()) return;

        if(EventSystem.current != null)
        {
            var current = EventSystem.current.currentSelectedGameObject;
            if(current != null)
            {
                lastSelectedButton = current;
            }
        }
    }

    private void SetupNavigation()
    {
        Navigation attackNav = new Navigation();
        attackNav.mode = Navigation.Mode.Explicit;
        attackNav.selectOnUp = escapeButton;
        attackNav.selectOnDown = sorceryButton;
        attackNav.selectOnLeft = null;
        attackNav.selectOnRight = null;
        attackButton.navigation = attackNav;

        Navigation sorceryNav = new Navigation();
        sorceryNav.mode = Navigation.Mode.Explicit;
        sorceryNav.selectOnUp = attackButton;
        sorceryNav.selectOnDown = itemButton;
        sorceryNav.selectOnLeft = null;
        sorceryNav.selectOnRight = null;
        sorceryButton.navigation = sorceryNav;

        Navigation itemNav = new Navigation();
        itemNav.mode = Navigation.Mode.Explicit;
        itemNav.selectOnUp = sorceryButton;
        itemNav.selectOnDown = escapeButton;
        itemNav.selectOnLeft = null;
        itemNav.selectOnRight = null;
        itemButton.navigation = itemNav;

        Navigation escapeNav = new Navigation();
        escapeNav.mode = Navigation.Mode.Explicit;
        escapeNav.selectOnUp = itemButton;
        escapeNav.selectOnDown = attackButton;
        escapeNav.selectOnLeft = null;
        escapeNav.selectOnRight = null;
        escapeButton.navigation = escapeNav;
    }

    public void Show()
    {
        root.SetActive(true);

        if(EventSystem.current != null)
        {
            GameObject target  = lastSelectedButton != null
                ? lastSelectedButton
                : attackButton.gameObject;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target);
        }

        if(BattleHelpLog.Instance != null)
        {
            BattleHelpLog.Instance.Show("コマンドを選択してください");
        }
    }

    public void Hide()
    {
        root.SetActive(false);

        if(EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public bool IsVisible()
    {
        return root != null && root.activeSelf;
    }

    public void ResetSelection()
    {
        lastSelectedButton = null;
    }

    private void OnAttack()
    {
        StartCoroutine(SelectTargetFlow());
    }

    private IEnumerator SelectTargetFlow()
    {
        Hide();

        if(BattleHelpLog.Instance != null)
        {
            BattleHelpLog.Instance.Hide();
        }

        BattleTargetUI.Instance.SetAttackMode();
        BattleTargetUI.Instance.Show();

        yield return BattleLogUI.Instance.ShowLogAndWait("攻撃する敵を選択してください");
    }

    private void OnMagic()
    {
        StartCoroutine(OpenMagicFlow());
    }

    private IEnumerator OpenMagicFlow()
    {
        Hide();

        BattleMagicUI.Instance.Show();

        if(BattleHelpLog.Instance != null)
        {
            BattleHelpLog.Instance.Show("魔法を選択してください");
        }

        yield break;
    }

    private void OnItem()
    {
        StartCoroutine(OpenItemFlow());
    }

    private IEnumerator OpenItemFlow()
    {
        Hide();

        BattleItemUI.Instance.Show();

        BattleHelpLog.Instance.SetMessage("アイテムを選択してください");
        yield break;
    }

    private void OnEscape()
    {
        if(BattleHelpLog.Instance != null)
        {
            BattleHelpLog.Instance.Hide();
        }

        BattleManager.Instance.SetCommand(new EscapeCommand());
        Hide();
    }
}
