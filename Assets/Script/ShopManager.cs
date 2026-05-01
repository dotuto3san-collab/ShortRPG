using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using JetBrains.Annotations;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }
    public enum ShopState
    {
        // 買う・売る・出るを選択する画面
        ActionSelect,
        // 商品リストからアイテムを選択する画面
        ItemSelection,
        // 個数確認や性能確認する画面
        BuyConfirm      
    }

    public ShopState CurrentState { get; private set; }

    [Header("UIパネル設定")]
    [SerializeField] private GameObject shopMenuPanel;
    [SerializeField] private SellShopUI sellShopUI;

    [Header("商品リスト生成設定")]
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform itemListContainer;

    [Header("購入確認UI")]
    [SerializeField] private GameObject buyConfirmPanel;
    [SerializeField] private TMPro.TextMeshProUGUI confirmItemName;
    [SerializeField] private TMPro.TextMeshProUGUI amountText;
    [SerializeField] private GameObject buyConfirmFirstSelected;

    [Header("購入金額表示UI")]
    [SerializeField] private TMPro.TextMeshProUGUI itemSelectionGoldText;
    [SerializeField] private TMPro.TextMeshProUGUI boughtGoldText;
    [SerializeField] private TMPro.TextMeshProUGUI totalPriceText;

    private int currentAmount = 1;
    private int maxAmount = 1;

    private float inputTimer = 0f;
    private float inputInterval = 0.15f;
    private float firstInputDelay = 0.3f;

    private bool isHolding = false;

    private List<ItemData> currentShopInventory;

    public ItemData SelectedItem { get; private set; }

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if(shopMenuPanel != null) shopMenuPanel.SetActive(false);
        if(buyConfirmPanel != null) buyConfirmPanel.SetActive(false);
        
        if(sellShopUI != null)
        {
            sellShopUI.Close();
        }
    }
    public void OpenShop(List<ItemData>inventory)
    {
        currentShopInventory = inventory;
        CurrentState = ShopState.ItemSelection;
        OpenItemSelection();
    }

    public void OpenItemSelection()
    {
        Debug.Log("Shop: OpenItemSelection");

        if(shopMenuPanel == null) return;

        InputManager.Instance.IgnoreNextSubmit();

        shopMenuPanel.SetActive(true);
        CurrentState = ShopState.ItemSelection;
        RefreshShopList();
        UpdateItemSelectionGoldUI();
    }

    public void OpenBuyConfirm()
    {
        InputManager.Instance.IgnoreNextSubmit();

        int playerGold = GameManager.Instance.Money;
        int price = SelectedItem.buyPrice;

        int maxStack = 99;

        int capacity = InventoryManager.Instance.GetRemainingCapacity(SelectedItem);

        if(capacity <= 0)
        {
            Debug.Log("これ以上持てません");
            return;
        }

        int maxByGold;

        if(price <= 0)
        {
            maxByGold = maxStack;
        }
        else
        {
            maxByGold = playerGold / price;
            maxAmount = Mathf.Clamp(maxByGold, 1 , maxStack);
        }

        maxAmount = Mathf.Min(maxByGold, capacity);

        maxAmount = Mathf.Clamp(maxAmount, 1 , maxStack);

        currentAmount = 1;

        UpdateAmountUI();
        UpdateMoneyUI();

        if (SelectedItem == null) return;

        CurrentState = ShopState.BuyConfirm;
        SetItemSelectionNavigetion(false);

        if (buyConfirmPanel != null)
        {
            buyConfirmPanel.SetActive(true);
        }

        if (confirmItemName != null)
        {
            confirmItemName.text = SelectedItem.itemName;
        }

        if(buyConfirmFirstSelected != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buyConfirmFirstSelected);
        }
    }

    private void UpdateAmountUI()
    {
        if(amountText != null)
        {
            amountText.text = currentAmount.ToString();
        }
    }

    private void UpdateItemSelectionGoldUI()
    {
        int playerGold = GameManager.Instance.Money;

        if(itemSelectionGoldText != null)
        {
            itemSelectionGoldText.text = playerGold.ToString();
        }
    }

    private void UpdateMoneyUI()
    {
        int playerGold = GameManager.Instance.Money;

        int totalPrice = 0;

        if(SelectedItem != null)
        {
            totalPrice = SelectedItem.buyPrice * currentAmount;
        }

        int remainingGold = playerGold - totalPrice;

        if(boughtGoldText != null)
        {
            boughtGoldText.text = remainingGold.ToString();
        }

        if(totalPriceText != null)
        {
            totalPriceText.text = totalPrice.ToString();
        }
    }

    public void HandleInput()
    {
        if(CurrentState != ShopState.BuyConfirm) return;

        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");

        int delta = 0;

        if(v > 0)
        {
            delta = 1;
        }
        else if(v < 0)
        {
            delta = -1;
        }
        if(h > 0)
        {
            delta = 10;
        }
        else if(h < 0)
        {
            delta = -10;
        }

        if(delta == 0)
        {
            isHolding = false;
            inputTimer = 0f;
            return;
        }

        if (!isHolding)
        {
            ChangeAmount(delta);
            isHolding = true;
            inputTimer = firstInputDelay;
            return;
        }

        inputTimer -= Time.deltaTime;

        if(inputTimer < 0f)
        {
            ChangeAmount(delta);
            inputTimer = inputInterval;
        }
    }

    private void ChangeAmount(int delta)
    {
        int prev = currentAmount;

        currentAmount += delta;

        if(prev == 1 && delta < 0)
        {
            currentAmount = maxAmount;
        }
        else if(prev == maxAmount &&  delta > 0)
        {
            currentAmount = 1;
        }

        currentAmount = Mathf.Clamp(currentAmount, 1, maxAmount);

        UpdateAmountUI();
        UpdateMoneyUI();
    }

    public void BackToActionSelect()
    {
        if(shopMenuPanel != null) shopMenuPanel.SetActive(false);
        CurrentState = ShopState.ActionSelect;
    }

    public void CloseShop()
    {
        if (shopMenuPanel != null) shopMenuPanel.SetActive(false);

        GameManager.Instance.ChangeState(GameState.Exploring);

        if (InkManager.Instance != null)
        {
            InkManager.Instance.FinishStory();
        }
    }

    public void OpenSellShop()
    {
        if(shopMenuPanel != null)
        {
            shopMenuPanel.SetActive(false);
        }
        if(sellShopUI == null)
        {
            Debug.LogError("SellShopUIがShopManagerに設定されていません");
            return;
        }

        sellShopUI.gameObject.SetActive(true);
        sellShopUI.Open();
    }

    private void RefreshShopList()
    {
        foreach(Transform child in itemListContainer)
        {
            Destroy(child.gameObject);
        }

        if (currentShopInventory == null || currentShopInventory.Count == 0) return;

        GameObject firstButton = null;

        for(int i = 0; i < currentShopInventory.Count; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemListContainer);
            ItemSlot slotScript = slotObj.GetComponent<ItemSlot>();

            if (slotScript != null)
            {
                slotScript.Setup(currentShopInventory[i], OnItemSelected);
            }

            if(i == 0) firstButton = slotObj;
        }

        if(firstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstButton);
        }
    }

    private void OnItemSelected(ItemData data)
    {
        SetSelectedItem(data);
        OpenBuyConfirm();
    }

    public void SetSelectedItem(ItemData data)
    {
        SelectedItem = data;
    }

    void SetItemSelectionNavigetion(bool enable)
    {
        foreach(Transform child in itemListContainer)
        {
            Selectable selectable = child.GetComponent<Selectable>();
            if(selectable == null) continue;

            Navigation nav = selectable.navigation;

            if (enable)
            {
                nav.mode = Navigation.Mode.Automatic;
            }
            else
            {
                nav.mode = Navigation.Mode.None;
            }

            selectable.navigation = nav;
        }
    }

    public SellShopUI GetSellShopUI()
    {
        return sellShopUI;
    }

    public void ConfirmBuy()
    {
        if(SelectedItem == null) return;

        if (InventoryManager.Instance.IsFull(SelectedItem))
        {
            Debug.Log("これ以上持てません");
            return;
        }

        int totalPrice = SelectedItem.buyPrice * currentAmount;

        if (GameManager.Instance.SpendMoney(totalPrice))
        {
            InventoryManager.Instance.AddItem(SelectedItem, currentAmount);

            CloseBuyConfirm();
        }
        else
        {
            Debug.Log("お金が足りない");
        }
    }

    public void CloseBuyConfirm()
    {
        InputManager.Instance.IgnoreNextSubmit();

        if(buyConfirmPanel != null)
        {
            buyConfirmPanel.SetActive(false);
        }

        CurrentState = ShopState.ItemSelection;
        SetItemSelectionNavigetion(true);

        UpdateItemSelectionGoldUI();

        if(itemListContainer.childCount > 0 && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(itemListContainer.GetChild(0).gameObject);
        }
    }

    public void CloseItemSelection()
    {
        InputManager.Instance.IgnoreNextSubmit();

        if (shopMenuPanel != null)
        {
            shopMenuPanel.SetActive(false);
        }

        if(EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        CurrentState = ShopState.ActionSelect;

        if(InkManager.Instance != null)
        {
            InkManager.Instance.ReturnToShopMain();
        }
    }
    public void CloseSellShop()
    {
        if(sellShopUI != null)
        {
            sellShopUI.Close();
        }

        CurrentState = ShopState.ActionSelect;

        if(InkManager.Instance != null)
        {
            InkManager.Instance.ReturnToShopMain();
        }
    }

    public void ExitShop()
    {
        if(EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if(InkManager.Instance != null)
        {
            InkManager.Instance.FinishStory();
        }

        CurrentState = ShopState.ActionSelect;
    }
}
