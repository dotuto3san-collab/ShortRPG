using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemSlot : MonoBehaviour,ISelectHandler,ISubmitHandler
{
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image iconImage;

    private ItemData itemData;
    private System.Action<ItemData> onSubmitAction;

    public void Setup(ItemData data, System.Action<ItemData> onSubmit)
    {
        itemData = data;
        onSubmitAction = onSubmit;

        if(itemNameText != null) itemNameText.text = data.itemName;
        if (priceText != null) priceText.text = data.buyPrice.ToString();
        if(iconImage != null && data.icon != null) iconImage.sprite = data.icon;
    }
    public void OnSelect(BaseEventData eventData)
    {
        if(ShopManager.Instance != null)
        {
            ShopManager.Instance.SetSelectedItem(itemData);
        }
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if(itemData != null && onSubmitAction != null)
        {
            onSubmitAction.Invoke(itemData);
        }
    }
}
