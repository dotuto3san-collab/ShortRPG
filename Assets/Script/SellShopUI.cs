using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class SellShopUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private GameObject amountUIRoot;
    [SerializeField] private GameObject priceUIRoot;
    [SerializeField] private GameObject lockoverlay;
    [SerializeField] private GameObject sellButton;

    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI focusItemNameText;
    [SerializeField] private TextMeshProUGUI focusItemPriceText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI totalPriceText;
    [SerializeField] private TextMeshProUGUI afterMoneyText;

    [SerializeField] private GameObject cannotSellTextRoot;
    [SerializeField] private GameObject equippedLockTextRoot;

    [SerializeField] private Image rankImage;
    [SerializeField] private RarityIconDatabase rarityDB;

    private InventoryItem selectedItem;

    private int currentAmount = 1;
    private int maxAmount = 1;

    private int lastSelectedIndex = 0;

    List<Selectable> selectableList = new List<Selectable>();

    void Awake()
    {
        if(panel != null)
        {
            panel.SetActive(false);
        }
        if(confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
        if(amountUIRoot != null) amountUIRoot.SetActive(false);
        if(priceUIRoot != null) priceUIRoot.SetActive(false);
        if(lockoverlay != null) lockoverlay.SetActive(false);
        if(sellButton != null) sellButton.SetActive(false);

        if(cannotSellTextRoot != null) cannotSellTextRoot.SetActive(false);
        if (equippedLockTextRoot != null) equippedLockTextRoot.SetActive(false);
    }

    void Update()
    {
        if (!IsOpen() || IsConfirmOpen()) return;
        if(EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject != null)
        {
            UpdateSelectionScroll();
            UpdateFocusedItemDisplay();
        }
    }

    public void Open()
    {
        panel.SetActive(true);
        confirmPanel.SetActive(false);
        amountUIRoot.SetActive(false);
        priceUIRoot.SetActive(false);
        lockoverlay.SetActive(false);
        sellButton.SetActive(false);
        selectedItem = null;
        Refresh();
    }

    public bool IsOpen()
    {
        return panel != null && panel.activeSelf;
    }

    public bool IsConfirmOpen()
    {
        return confirmPanel != null && confirmPanel.activeSelf;
    }

    private void Refresh()
    {
        if(scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        foreach(Transform child in container)
        {
            Destroy(child.gameObject);
        }
        selectableList.Clear();

        var items = InventoryManager.Instance.GetItems();

        GameObject first = null;

        foreach(var inv in items)
        {
            bool isEquipped = false;

            if(EquipmentManager.Instance != null && inv.itemData.equipData != null)
            {
                var equipped = EquipmentManager.Instance.GetEquipped(inv.itemData.equipData.equipType);
                if (equipped == inv.itemData)
                {
                    isEquipped = true;
                }
            }

            if (inv.amount <= 0 && !isEquipped) continue;

            var obj = Instantiate(itemSlotPrefab, container);
            var row = obj.GetComponent<ItemRowUI>();
            if(row == null)
            {
                Debug.LogError("ItemRowUIÇ™ïtÇ¢ÇƒÇ¢Ç‹ÇπÇÒ");
                continue;
            }

            row.Setup(inv);
            row.SetOnSubmitAction(selectedItem => OnItemSelected(selectedItem.itemData));

            var selectable = obj.GetComponent<Selectable>();
            if(selectable == null)
            {
                Debug.LogError("SelectableÇ™ïtÇ¢ÇƒÇ¢Ç‹ÇπÇÒ : " + obj.name);
                continue;
            }
            selectableList.Add(selectable);

            if(EquipmentManager.Instance != null && inv.itemData.equipData != null)
            {
                var equipped = EquipmentManager.Instance.GetEquipped(inv.itemData.equipData.equipType);
                if(equipped == inv.itemData)
                {
                    isEquipped = true;
                }
            }

            bool cannotSell = !inv.itemData.canSell || (isEquipped && inv.amount -1 <= 0);

            if (cannotSell)
            {
                var canvasGroup = obj.GetComponent<CanvasGroup>();
                if(canvasGroup == null)
                {
                    canvasGroup = obj.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0.5f;
            }

            if (first == null && selectable.interactable)
            {
                first = obj;
            }
        }
        
        if(EventSystem.current != null && selectableList.Count > 0)
        {
            int index = Mathf.Clamp(lastSelectedIndex, 0, selectableList.Count - 1);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectableList[index].gameObject);
            UpdateFocusedItemDisplay();
        }

        int  count = selectableList.Count;
        if (count == 0) return;

        for(int i = 0; i < selectableList.Count; i++)
        {
            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = null,
                selectOnRight = null
            };
            
            if(count == 1)
            {
                nav.selectOnUp = selectableList[i];
                nav.selectOnDown = selectableList[i];
            }
            else
            {
                nav.selectOnUp = selectableList[(i - 1 + count) % count];
                nav.selectOnDown = selectableList[(i + 1) % count];
            }
            selectableList[i].navigation = nav;
        }
    }

    private void OnItemSelected(ItemData data)
    {
        if(EventSystem.current != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if(selected != null && selected.transform.IsChildOf(container))
            {
                lastSelectedIndex = selected.transform.GetSiblingIndex();
            }
        }
        SelectItem(data);
    }

    public void SelectItem(ItemData data)
    {
        var inv = InventoryManager.Instance.GetItems()
            .Find(i => i.itemData == data);

        if(inv == null) return;

        bool isEquippedZero = false;

        if(EquipmentManager.Instance != null && inv.itemData.equipData != null)
        {
            var equipped = EquipmentManager.Instance.GetEquipped(inv.itemData.equipData.equipType);
            if (equipped == inv.itemData && inv.amount <= 0)
            {
                isEquippedZero = true;
            }
        }

        if (!inv.itemData.canSell || isEquippedZero)
        {
            Debug.Log("îÑãpïsâ¬ÉAÉCÉeÉÄÇ≈Ç∑");
            return;
        }

        selectedItem = inv;

        OpenConfirm();
    }

    void UpdateFocusedItemDisplay()
    {
        if (focusItemNameText == null) return;
        if(EventSystem.current == null) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if(selected == null) return;

        ItemRowUI row = selected.GetComponent<ItemRowUI>();
        if(row == null) return;

        InventoryItem item = row.GetItem();
        if (item == null) return;

        focusItemNameText.text = item.itemData.itemName;

        bool isEquipped = false;

        if(EquipmentManager.Instance != null && item.itemData.equipData != null)
        {
            var equipped = EquipmentManager.Instance.GetEquipped(item.itemData.equipData.equipType);
            isEquipped = (equipped == item.itemData);
        }

        bool cannotSellByFlag = !item.itemData.canSell;
        bool cannotSellByEquip = isEquipped && item.amount - 1 <= 0;

        if(cannotSellTextRoot != null)
            cannotSellTextRoot.SetActive(cannotSellByFlag);

        if (equippedLockTextRoot != null)
            equippedLockTextRoot.SetActive(!cannotSellByFlag && cannotSellByEquip);

        if (focusItemPriceText != null)
        {
            if (!cannotSellByFlag && !cannotSellByEquip)
            {
                focusItemPriceText.text = item.itemData.sellPrice.ToString();
            }
            else
            {
                focusItemPriceText.text = "-";
            }
        }

        if(rankImage != null && rarityDB != null)
        {
            rankImage.sprite = rarityDB.GetIcon(item.itemData.rarity);
        }
    }

    void UpdateSelectionScroll()
    {
        if (scrollRect == null) return;
        if (EventSystem.current == null) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if(selected == null) return;

        RectTransform viewport = scrollRect.viewport;
        RectTransform contentRect = scrollRect.content;
        RectTransform target = selected.GetComponent<RectTransform>();

        if(target == null) return;

        Vector3[] targetCorners = new Vector3[4];
        Vector3[] viewCorners = new Vector3[4];

        target.GetWorldCorners(targetCorners);
        viewport.GetWorldCorners(viewCorners);

        float targetTop = targetCorners[1].y;
        float targetBottom = targetCorners[0].y;

        float viewTop = viewCorners[1].y;
        float viewBottom = viewCorners[0].y;

        float offset = 0f;

        float topMargin = 10f;
        float bottomMargin = -10f;

        if(targetTop > viewTop - topMargin)
        {
            offset = targetTop - (viewTop - topMargin);
        }
        else if(targetBottom < viewBottom - bottomMargin)
        {
            offset = targetBottom - (viewBottom - bottomMargin);
        }

        if(Mathf.Abs(offset) > 0.01f)
        {
            Vector2 pos = contentRect.anchoredPosition;
            pos.y -= offset;
            contentRect.anchoredPosition = pos;
        }
    }

    public void OpenConfirm()
    {
        bool isEquipped = false;
        if(EquipmentManager.Instance != null && selectedItem.itemData.equipData != null)
        {
            var equipped = EquipmentManager.Instance.GetEquipped(selectedItem.itemData.equipData.equipType);
            isEquipped = (equipped == selectedItem.itemData);
        }

        maxAmount = isEquipped ? selectedItem.amount - 1 : selectedItem.amount;

        if(maxAmount <= 0)
        {
            Debug.Log("îÑãpÇ≈Ç´ÇÈêîÇ™Ç†ÇËÇ‹ÇπÇÒ");
            return;
        }

        currentAmount = 1;

        confirmPanel.SetActive(true);
        if (amountUIRoot != null) amountUIRoot.SetActive(true);
        if (priceUIRoot != null) priceUIRoot.SetActive(true);
        if (lockoverlay != null) lockoverlay.SetActive(true);

        if (sellButton != null)
        {
            sellButton.SetActive(true);

            var button = sellButton.GetComponent<Button>();

            if(button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Confirm);
            }

            if(EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(sellButton);
            }
        }

        SetItemRowNavigation(false);

        itemNameText.text = selectedItem.itemData.itemName;

        if (amountText != null)
        {
            amountText.text = currentAmount.ToString();
        }

        UpdateUI();
    }

    public void ChangeAmount(int delta)
    {
        currentAmount += delta;

        if (currentAmount < 1) currentAmount = maxAmount;
        if (currentAmount > maxAmount) currentAmount = 1;

        UpdateUI();
    }

    public void UpdateUI()
    {
        amountText.text = currentAmount.ToString();
        int total = selectedItem.itemData.sellPrice * currentAmount;
        totalPriceText.text = total.ToString();

        int afterMoney = GameManager.Instance.Money + total;

        if(afterMoneyText != null)
        {
            afterMoneyText.text = afterMoney.ToString();
        }
    }

    public void Confirm()
    {
        if(selectedItem == null)
        {
            Debug.LogError("selectedItemÇ™nullÇ≈Ç∑");
            return;
        }

        if(currentAmount <= 0)
        {
            Debug.LogError("îÑãpêîÇ™ïsê≥Ç≈Ç∑");
            return;
        }

        if(currentAmount > selectedItem.amount)
        {
            Debug.LogError("èäéùêîà»è„ÇîÑãpÇµÇÊÇ§Ç∆ÇµÇƒÇ¢Ç‹Ç∑");
            return;
        }

        bool isEquipped = false;
        if(EquipmentManager.Instance != null && selectedItem.itemData.equipData != null)
        {
            var equipped = EquipmentManager.Instance.GetEquipped(selectedItem.itemData.equipData.equipType);
            isEquipped = (equipped == selectedItem.itemData);
        }

        if(isEquipped && (selectedItem.amount - currentAmount) <= 0)
        {
            Debug.Log("ëïîıíÜÇÃÇΩÇﬂîÑãpÇ≈Ç´Ç‹ÇπÇÒ");
            return;
        }

        if (!selectedItem.itemData.canSell)
        {
            Debug.LogError("îÑãpïsâ¬ÉAÉCÉeÉÄÇ≈Ç∑");
            return;
        }

        int total = selectedItem.itemData.sellPrice * currentAmount;

        bool willRemoveRow = (selectedItem.amount - currentAmount) <= 0;

        InventoryManager.Instance.RemoveItem(selectedItem.itemData, currentAmount);

        GameManager.Instance.AddMoney(total);

        confirmPanel.SetActive(false);
        if(amountUIRoot != null) amountUIRoot.SetActive(false);
        if(priceUIRoot != null) priceUIRoot.SetActive(false);
        if(lockoverlay != null) lockoverlay.SetActive(false);
        if(sellButton != null) sellButton.SetActive(false);

        Refresh();
    }

    void SetItemRowNavigation(bool enable)
    {
        foreach(Transform child in container)
        {
            var selectable = child.GetComponent<Selectable>();
            if(selectable == null) continue;

            var nav = selectable.navigation;
            nav.mode = enable ? Navigation.Mode.Automatic : Navigation.Mode.None;
            selectable.navigation = nav;
        }
    }
    
    public void CloseConfirm()
    {
        confirmPanel.SetActive(false);
        amountUIRoot.SetActive(false);
        priceUIRoot.SetActive(false);
        lockoverlay.SetActive(false);
        sellButton.SetActive(false);
        selectedItem = null;
        Refresh();
    }

    public void Close()
    {
        panel.SetActive(false);
        confirmPanel.SetActive(false);
        selectedItem = null;

        if(EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
