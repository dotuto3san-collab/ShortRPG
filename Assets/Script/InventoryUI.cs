using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private TextMeshProUGUI focusItemNameText;
    [SerializeField] private TextMeshProUGUI focusItemAmountText;

    [SerializeField] private Image rankImage;
    [SerializeField] private RarityIconDatabase rarityDB;

    public Transform content;
    public GameObject itemRowPrefab;

    private int lastSelectedIndex = 0;

    // Update is called once per frame
    void Update()
    {
        if (!IsOpen()) return;
        if(EventSystem.current == null) return;

        var selected = EventSystem.current.currentSelectedGameObject;
        if(selected == null)
        {
            if (focusItemNameText != null) focusItemNameText.text = "";
            if (focusItemAmountText != null) focusItemAmountText.text = "";
            return;
        }

        if (!selected.transform.IsChildOf(content)) return;

        UpdateFocusedItemDisplay();
        UpdateSelectionScroll();
    }

    void UpdateFocusedItemDisplay()
    {
        if(focusItemNameText == null &&  focusItemAmountText == null) return;
        if (EventSystem.current == null) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;

        ItemRowUI row = selected.GetComponent<ItemRowUI>();
        if (row == null) return;

        InventoryItem item = row.GetItem();
        if(item == null || item.itemData == null) return;

        if(focusItemNameText != null)
        {
            focusItemNameText.text = item.itemData.itemName;
        }
        if(focusItemAmountText != null)
        {
            int displayAmount = ItemDisplayHelper.GetDisplayAmount(item);
            bool isEquipped = ItemDisplayHelper.IsEquipped(item);

            focusItemAmountText.text = displayAmount.ToString();

            Color color = isEquipped ? new Color(0f, 0.4f, 1f) : Color.black;
            
            focusItemNameText.color = color;
            focusItemAmountText.color = color;
        }

        if (rankImage != null && rarityDB != null)
        {
            rankImage.sprite = rarityDB.GetIcon(item.itemData.rarity);
        }
    }

    void OnEnable()
    {
        StartCoroutine(DelayedRefresh());   
    }

    IEnumerator DelayedRefresh()
    {
        yield return null;

        if(InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManagerの初期化が間に合っていない");
            yield break;
        }

        Refresh();
    }

    public bool IsOpen()
    {
        return gameObject.activeInHierarchy;
    }

    void UpdateSelectionScroll()
    {
        if (scrollRect == null) return;
        if(EventSystem.current == null) return;

        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if(selected == null) return;

        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;
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
            Vector2 pos = content.anchoredPosition;
            pos.y -= offset;
            content.anchoredPosition = pos;
        }
    }

    public void Refresh()
    {
        if(InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager が未初期化のため Refresh をスキップ");
            return;
        }

        if (focusItemNameText != null) focusItemNameText.text = "";
        if (focusItemAmountText != null) focusItemAmountText.text = "";

        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        List<InventoryItem> items = InventoryManager.Instance.GetItems();

        Debug.Log($"Inventory Refresh: {items.Count} items");

        GameObject first = null;

        foreach (var item in items)
        {
            GameObject row = Instantiate(itemRowPrefab,content);

            if (first == null) first = row;

            ItemRowUI rowUI = row.GetComponent<ItemRowUI>();
            rowUI.Setup(item);
            rowUI.SetOnSubmitAction(OnItemSelected);
        }

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SetupNavigationNextFrame(first));
        }
        else
        {
            Debug.Log("InventoryUI is inactive. Coroutine skipped.");
        }
        
    }

    private InventoryItem selectedItem;

    void OnItemSelected(InventoryItem item)
    {
        if(item == null || item.itemData == null)
        {
            Debug.LogError("Item is null");
            return;
        }

        selectedItem = item;

        if(EventSystem.current != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(content))
            {
                lastSelectedIndex = selected.transform.GetSiblingIndex();
            }
        }

        if(ItemUseConfirmUI.Instance != null)
        {
            ItemUseConfirmUI.Instance.Show(item, OnConfirmUseItem);
        }
    }

    void OnConfirmUseItem(bool isYes)
    {
        if (!isYes) return;

        if(EventSystem.current != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(content))
            {
                lastSelectedIndex = selected.transform.GetSiblingIndex();
            }
        }

        if(EventSystem.current != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if(selected != null && selected.transform.IsChildOf(content))
            {
                lastSelectedIndex = selected.transform.GetSiblingIndex();
            }
        }

        if(selectedItem == null || selectedItem.itemData == null)
        {
            Debug.LogError("SelectedItem is null");
            return;
        }

        var itemData = selectedItem.itemData;

        if(itemData.equipData != null
            && itemData.equipData.equipType != EquipData.EquipType.None)
        {
            if(EquipmentManager.Instance == null)
            {
                Debug.LogError("EquipmentManager not found");
                return;
            }

            var equipType = itemData.equipData.equipType;
            var currentlyEquipped = EquipmentManager.Instance.GetEquipped(equipType);

            if(currentlyEquipped == itemData)
            {
                EquipmentManager.Instance.Unequip(equipType);
            }
            else
            {
                EquipmentManager.Instance.Equip(itemData);
            }

            Refresh();

            return;
        }

        if (!itemData.canUseInField)
        {
            Debug.Log("フィールドでは使えない");
            return;
        }

        if(itemData.useEffect == null)
        {
            Debug.LogError("ItemEffect is null");
            return;
        }

        StartCoroutine(UseItem(selectedItem));
    }

    IEnumerator UseItem(InventoryItem item)
    {
        var effect = item.itemData.useEffect;

        yield return effect.Apply(null,null);

        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(item.itemData, 1);
        }

        Refresh();

        var statusUI = FindFirstObjectByType<MenuStatusUI>();
        if(statusUI != null)
        {
            statusUI.UpdateHPDisplay();
        }
    }

    IEnumerator SetupNavigationNextFrame(GameObject first)
    {
        yield return null;

        List<Selectable> selectables = new List<Selectable>();

        foreach(Transform child in content)
        {
            Selectable s = child.GetComponent<Selectable>();
            if (s != null) selectables.Add(s);
        }

        if(selectables.Count == 0) yield break;

        for(int i = 0; i < selectables.Count; i++)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = selectables[(i - 1 + selectables.Count) % selectables.Count];
            nav.selectOnDown = selectables[(i + 1) % selectables.Count];

            nav.selectOnLeft = null;
            nav.selectOnRight = null;

            selectables[i].navigation = nav;
        }
        if(EventSystem.current != null && selectables.Count > 0)
        {
            int index = Mathf.Clamp(lastSelectedIndex, 0, selectables.Count - 1);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(selectables[index].gameObject);

            UpdateFocusedItemDisplay();
        }
    }
}
