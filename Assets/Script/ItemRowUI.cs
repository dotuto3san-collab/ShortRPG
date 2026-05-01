using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

public class ItemRowUI : MonoBehaviour, ISubmitHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI amountText;

    private InventoryItem item;
    private Action<InventoryItem> onSubmitAction;

    public void Setup(InventoryItem item)
    {
        this.item = item;

        nameText.text = item.itemData.itemName;
        amountText.text = item.amount.ToString();

        var selectable = GetComponent<Selectable>();
        if(selectable == null)
        {
            Debug.LogError("Selectable‚ª•t‚¢‚Ä‚Ü‚¹‚ñ: " + gameObject.name);
        }
    }

    public InventoryItem GetItem()
    {
        return item;
    }

    public void SetOnSubmitAction(Action<InventoryItem> callback)
    {
        onSubmitAction = callback;
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("‘I‘ð’†: " + item.itemData.itemName);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (item == null) return;

        onSubmitAction?.Invoke(item);
        Debug.Log("Œˆ’è: " + item.itemData.itemName);
    }
}
