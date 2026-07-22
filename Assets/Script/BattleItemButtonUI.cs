using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleItemButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;

    private InventoryItem item;
    private BattleItemUI parent;

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

    private void OnClick()
    {
        parent.OnItemSelected(item.itemData);
    }
}
