using System.Collections;
using System.Collections.Generic;
using Ink.Parsed;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleItemUI : MonoBehaviour
{
    public static BattleItemUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject itemButtonPrefab;

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
        if(InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager is null");
            return;
        }

        buttons.Clear();

        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }

        var items = InventoryManager.Instance.GetItems();

        GameObject firstButton = null;

        foreach (var invItem in items)
        {
            var itemData = invItem.itemData;

            if(!itemData.canUseInBattle) continue;

            GameObject obj = Instantiate(itemButtonPrefab, content);

            var ui = obj.GetComponent<BattleItemButtonUI>();
            ui.Setup(invItem, this);

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

    public void OnItemSelected(ItemData item)
    {

        if(item == null)
        {
            Debug.LogError("Item is null.");
            return;
        }

        HandleItemSelection(item);
    }

    public bool IsActive()
    {
        return root != null && root.activeSelf;
    }

    private void HandleItemSelection(ItemData item)
    {
        if(item.useEffect == null)
        {
            Debug.LogError("ItemEffect is null.");
            return;
        }
        
        switch (item.useEffect.targetType)
        {
            case ItemEffect.TargetType.Self:
                BattleManager.Instance.SetTarget(BattleManager.Instance.player);
                BattleManager.Instance.SetCommand(new UseItemCommand(item));
                if(BattleHelpLog.Instance != null)
                {
                    BattleHelpLog.Instance.Hide();
                }
                Hide();
                break;

            case ItemEffect.TargetType.Enemy:
                StartCoroutine(SelectItemTargetFlow(item));
                break;

            case ItemEffect.TargetType.AllEnemies:

                if(BattleHelpLog.Instance != null)
                {
                    BattleHelpLog.Instance.Hide();
                }

                BattleManager.Instance.SetTarget(null);
                BattleManager.Instance.SetCommand(new UseItemCommand(item));
                Hide();
                break;

            //case ItemEffect.TargetType.Ally:
            /*case ItemEffect.TargetType.AllAllies:
                Debug.LogWarning("ShortRPGでは使用しておりません");
                return;
                */

            default:
                Debug.LogError("未知のTargetType" + item.useEffect.targetType);
                break;
        }
    }

    private IEnumerator SelectItemTargetFlow(ItemData item)
    {
        Hide();

        if(BattleHelpLog.Instance != null)
        {
            BattleHelpLog.Instance.Hide();
        }

        BattleTargetUI.Instance.SetItem(item);
        BattleTargetUI.Instance.Show();

        yield return BattleLogUI.Instance.ShowLogAndWait("対象の敵を選択してください");
    }

    private System.Collections.IEnumerator SelectEnemyTarget(ItemData item)
    {
        Hide();

        BattleTargetUI.Instance.Show();

        yield return BattleLogUI.Instance.ShowLogAndWait("対象の敵を選択してください");

        yield return new WaitUntil(() => BattleManager.Instance != null
            && BattleManager.Instance.enemies != null
            && BattleManager.Instance.player != null);

        yield break;
    }
}
