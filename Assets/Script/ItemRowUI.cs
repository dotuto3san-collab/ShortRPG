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

        int displayAmount = ItemDisplayHelper.GetDisplayAmount(item);
        amountText.text = displayAmount.ToString();

        var selectable = GetComponent<Selectable>();
        if(selectable == null)
        {
            Debug.LogError("Selectable‚ª•t‚¢‚Ä‚Ü‚¹‚ñ: " + gameObject.name);
        }

        UpdateEquipColor();
    }

    private void UpdateEquipColor()
    {
        if(EquipmentManager.Instance ==  null || item == null || item.itemData == null)
        {
            SetDefaultColor();
            return;
        }

        var equipType = item.itemData.equipData?.equipType;

        if(equipType == null)
        {
            SetDefaultColor();
            return;
        }

        var equipped = EquipmentManager.Instance.GetEquipped(equipType.Value);

        if(equipped == item.itemData)
        {
            nameText.color = new Color(0f, 0.4f, 1f);
            amountText.color = new Color(0, 0.4f, 1f);
        }
        else
        {
            SetDefaultColor();
            return;
        }
    }

    void SetDefaultColor()
    {
        nameText.color = Color.black;
        amountText.color = Color.black;
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
