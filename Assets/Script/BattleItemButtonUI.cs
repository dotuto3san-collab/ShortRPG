using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BattleItemButtonUI : MonoBehaviour, ISelectHandler
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;

    private InventoryItem item;
    private BattleItemUI parent;

    public ItemData ItemData => item?.itemData;

    public void Setup(InventoryItem item, BattleItemUI parent)
    {
        this.item = item;
        this.parent = parent;

        nameText.text = item.itemData.itemName;
        amountText.text = item.amount.ToString();

        var button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void ShowFocus()
    {
        if(item == null)
        {
            return;
        }

        if(parent != null)
        {
            parent.OnItemFocused(
                item.itemData,
                transform.GetSiblingIndex()
            );
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(item == null)
        {
            return;
        }

        if(parent != null)
        {
            parent.OnItemFocused(
                item.itemData,
                transform.GetSiblingIndex()
            );
        }
    }

    private void OnClick()
    {
        if(item == null)
        {
            return;
        }

        if(parent != null)
        {
            parent.OnItemSelected(item.itemData);
        }
    }
}
