using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleMagicUI : MonoBehaviour
{
    public static BattleMagicUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject magicButtonPrefab;

    List<Button> buttons = new List<Button>();

    void Awake()
    {
        Instance = this;
        root.SetActive(false);
    }

    public void Show()
    {
        root.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    void Update()
    {
        if(!root.activeSelf) return;

        if(Input.GetKeyDown(KeyCode.X) ||
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

        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }

        GameObject firstButton = null;

        var magics = PlayerStatus.Instance.GetLearnedMagics();

        foreach (var magic in magics)
        {
            if(magic == null) continue;

            GameObject obj = Instantiate(magicButtonPrefab, content);

            var ui = obj.GetComponent<BattleMagicButtonUI>();
            ui.Setup(magic, this);

            var btn = obj.GetComponent<Button>();
            buttons.Add(btn);

            var nav = btn.navigation;
            nav.mode = Navigation.Mode.Explicit;
            btn.navigation = nav;

            if(firstButton == null)
            {
                firstButton = obj;
            }
        }

        int colume = 3;
        int count = buttons.Count;

        for(int i = 0; i < count; i++)
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
            if(down >= count)
            {
                down = col;
                if (down >= count) down = i;
            }

            int up = i - colume;
            if(up < 0)
            {
                int lastRowStart = ((count - 1) / colume) * colume;
                int candidate = lastRowStart + col;

                while(candidate >= count && candidate >= col)
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

        if(firstButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
    }

    public void OnMagicSelected(MagicData magic)
    {

        if(magic == null)
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

                if(BattleHelpLog.Instance != null)
                {
                    BattleHelpLog.Instance.Hide();
                }

                Hide();
                break;

            case MagicTargetType.EnemySingle:
                StartCoroutine(SelectMagicTargetFlow(magic));
                break;

            default:
                Debug.LogError("–¢’m‚ÌTargetType" + magic.targetType);
                break;
        }
    }

    private IEnumerator SelectMagicTargetFlow(MagicData magic)
    {
        Hide();

        if(BattleHelpLog.Instance != null)
        {
            BattleHelpLog.Instance.Hide();
        }

        BattleTargetUI.Instance.SetMagic(magic);
        BattleTargetUI.Instance.Show();

        yield return BattleLogUI.Instance.ShowLogAndWait("‘ÎÛ‚Ì“G‚ð‘I‘ð‚µ‚Ä‚­‚¾‚³‚¢");
    }

    private System.Collections.IEnumerator SelectEnemyTarget(ItemData item)
    {
        Hide();

        BattleTargetUI.Instance.Show();

        yield return BattleLogUI.Instance.ShowLogAndWait("‘ÎÛ‚Ì“G‚ð‘I‘ð‚µ‚Ä‚­‚¾‚³‚¢");

        yield return new WaitUntil(() => BattleManager.Instance != null
            && BattleManager.Instance.enemies != null
            && BattleManager.Instance.player != null);

        yield break;
    }
}
