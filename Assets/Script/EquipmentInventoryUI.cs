using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Unity.VisualScripting;

public class EquipmentInventoryUI : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject itemRowPrefab;
    [SerializeField] private EquipmentStatusUI equipmentStatusUI;

    private int lastselectedIndex = 0;

    void OnEnable()
    {
        if(EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged += Refresh;
        }
        
        StartCoroutine(DelayedRefresh());
    }

    void OnDisable()
    {
        if(EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= Refresh;
        }
    }

    IEnumerator DelayedRefresh()
    {
        yield return null;

        if(InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found");
            yield break;
        }

        Refresh();
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (EventSystem.current == null) return;

        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;

        if(!selected.transform.IsChildOf(content)) return;

        UpdateSelectionScroll();

        var rowUI = selected.GetComponent<ItemRowUI>();
        if (rowUI != null && equipmentStatusUI != null)
        {
            var item = rowUI.GetItem();
            if (item != null)
            {
                equipmentStatusUI.ShowPreview(item.itemData);
            }
        }
    }

    void UpdateSelectionScroll()
    {
        if(scrollRect == null) return;
        if(EventSystem.current == null) return;

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

    public void Refresh()
    {
        foreach(Transform child in content)
        {
            Destroy(child.gameObject);
        }

        List<InventoryItem> items = InventoryManager.Instance.GetItems();

        GameObject first = null;

        foreach (var item in items)
        {
            if (item.itemData == null) continue;
            if (item.itemData.equipData == null) continue;
            if (item.itemData.equipData.equipType == EquipData.EquipType.None) continue;

            GameObject row = Instantiate(itemRowPrefab, content);

            if (first == null) first = row;

            ItemRowUI rowUI = row.GetComponent<ItemRowUI>();
            rowUI.Setup(item);

            rowUI.SetOnSubmitAction(OnItemSelected);

            
        }

        if(gameObject.activeInHierarchy)
        {
            StartCoroutine(SetupNavigationNextFrame(first));
        }

        void OnItemSelected(InventoryItem item)
        {
            if(item == null || item.itemData == null)
            {
                Debug.LogError("Item is null");
                return;
            }

            if(EventSystem.current != null)
            {
                var selected = EventSystem.current.currentSelectedGameObject;
                if(selected != null && selected.transform.IsChildOf(content))
                {
                    lastselectedIndex = selected.transform.GetSiblingIndex();
                }
            }

            if(EquipmentManager.Instance == null)
            {
                Debug.LogError("EquipmentManager not found");
                return;
            }

            var equipType = item.itemData.equipData.equipType;

            var current = EquipmentManager.Instance.GetEquipped(equipType);

            if(current == item.itemData)
            {
                EquipmentManager.Instance.Unequip(equipType);
            }
            else
            {
                EquipmentManager.Instance.Equip(item.itemData);
            }

            Refresh();
        }

        IEnumerator SetupNavigationNextFrame(GameObject first)
        {
            yield return null;

            List<Selectable> selectables = new List<Selectable>();

            foreach (Transform child in content)
            {
                Selectable s = child.GetComponent<Selectable>();
                if(s != null) selectables.Add(s);
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
                int index = Mathf.Clamp(lastselectedIndex, 0, selectables.Count - 1);

                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(selectables[index].gameObject);
            }
        }
    }
}
