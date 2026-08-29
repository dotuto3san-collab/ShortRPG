using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

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

    [Header("アイテム情報")]
    [SerializeField] private Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;

    [Header("商品情報パネル")]
    [SerializeField] private GameObject normalItemPanel;
    [SerializeField] private GameObject equipmentItemPanel;

    [Header("商品フォーカス情報")]
    [SerializeField] private TextMeshProUGUI selectedItemNameText;
    [SerializeField] private TextMeshProUGUI selectedItemEffectText;

    [Header("通常アイテムパネル - プレイヤー情報")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private Slider playerHPBar;
    [SerializeField] private Image playerIconImage;

    [Header("装備アイテムパネル - 現在のステータス")]
    [SerializeField] private TextMeshProUGUI equipmentAttackText;
    [SerializeField] private TextMeshProUGUI equipmentDefenseText;
    [SerializeField] private TextMeshProUGUI equipmentChargeText;

    [Header("装備アイテムパネル - 現在の装備")]
    [SerializeField] private TextMeshProUGUI currentWeaponText;
    [SerializeField] private TextMeshProUGUI currentArmorText;

    [Header("装備アイテムパネル - 装備後ステータス")]
    [SerializeField] private TextMeshProUGUI previewAttackText;
    [SerializeField] private TextMeshProUGUI previewDefenseText;
    [SerializeField] private TextMeshProUGUI previewChargeText;

    [SerializeField] private Image rankImage;
    [SerializeField] private RarityIconDatabase rarityDB;

    [Header("購入金額表示UI")]
    [SerializeField] private TMPro.TextMeshProUGUI itemSelectionGoldText;
    [SerializeField] private TMPro.TextMeshProUGUI boughtGoldText;
    [SerializeField] private TMPro.TextMeshProUGUI totalPriceText;

    private int currentAmount = 1;
    private int maxAmount = 1;

    private int lastSelectedIndex = 0;

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

        if(normalItemPanel != null)
        {
            normalItemPanel.SetActive(false);
        }

        if(equipmentItemPanel != null)
        {
            equipmentItemPanel.SetActive(false);
        }
    }

    public void OpenShop(List<ItemData>inventory)
    {
        currentShopInventory = inventory;

        lastSelectedIndex = 0;

        CurrentState = ShopState.ItemSelection;
        OpenItemSelection();
    }

    public void OpenItemSelection()
    {
        Debug.Log("Shop: OpenItemSelection");

        if(shopMenuPanel == null) return;

        shopMenuPanel.SetActive(true);
        CurrentState = ShopState.ItemSelection;
        RefreshShopList();
        UpdateItemSelectionGoldUI();
    }

    public void OpenBuyConfirm()
    {
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

        if (itemIcon != null)
        {
            itemIcon.sprite = SelectedItem.icon;
            itemIcon.enabled = SelectedItem.icon != null;
        }

        if(itemDescription != null)
        {
            itemDescription.text = SelectedItem.description;
        }

        if (rankImage != null && rarityDB != null && SelectedItem != null)
        {
            rankImage.sprite = rarityDB.GetIcon(SelectedItem.rarity);
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

        StartCoroutine(SetupNavigationNextFrame());

        if(firstButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton);

            var firstSlot = firstButton.GetComponent<ItemSlot>();

            if(firstSlot != null)
            {
                firstSlot.ShowFocus();
            }
        }
    }

    private IEnumerator SetupNavigationNextFrame()
    {
        yield return null;
        int count = itemListContainer.childCount;

        for(int i = 0; i < count; i++)
        {
            var selectable = itemListContainer.GetChild(i).GetComponent<Selectable>();
            if(selectable == null) continue;

            var nav = new Navigation()
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = null,
                selectOnRight = null
            };

            nav.selectOnUp = i == 0
                ? itemListContainer.GetChild(count - 1).GetComponent<Selectable>()
                : itemListContainer.GetChild(i - 1).GetComponent<Selectable>();

            nav.selectOnDown = i == count - 1
                ? itemListContainer.GetChild(0).GetComponent<Selectable>()
                : itemListContainer.GetChild(i + 1).GetComponent<Selectable>();

            selectable.navigation = nav;
        }

        if(count > 0 && EventSystem.current != null)
        {
            int index = Mathf.Clamp(lastSelectedIndex, 0, count - 1);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(itemListContainer.GetChild(index).gameObject);
        }
    }

    private void OnItemSelected(ItemData data)
    {
        if(EventSystem.current != null)
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(itemListContainer))
            {
                lastSelectedIndex = selected.transform.GetSiblingIndex();
            }
        }

        SetSelectedItem(data);
        OpenBuyConfirm();
    }

    public void SetSelectedItem(ItemData data)
    {
        SelectedItem = data;
    }

    public void OnItemFocused(ItemData data)
    {
        if(data == null)
        {
            return;
        }

        SelectedItem = data;

        if(selectedItemNameText != null)
        {
            selectedItemNameText.text = data.itemName;
        }

        if(selectedItemEffectText != null)
        {
            selectedItemEffectText.text = data.effectText;
        }

        bool isEquipment =
            data.equipData != null &&
            data.equipData.equipType != EquipData.EquipType.None;

        if (!isEquipment)
        {
            if(normalItemPanel != null)
            {
                normalItemPanel.SetActive(true);
            }

            if(equipmentItemPanel != null)
            {
                equipmentItemPanel.SetActive(false);
            }

            UpdateNormalItemPlayerStatus();

            return;
        }

        if(normalItemPanel != null)
        {
            normalItemPanel.SetActive(false);
        }

        if(equipmentItemPanel != null)
        {
            equipmentItemPanel.SetActive(true);
        }

        UpdateEquipmentItemStatus(data);
    }

    private void UpdateNormalItemPlayerStatus()
    {
        if(PlayerStatus.Instance == null)
        {
            Debug.LogWarning("ShopManager: PlayerStatus.Instance が存在しません");
            return;
        }

        if(playerNameText != null)
        {
            playerNameText.text = PlayerStatus.Instance.GetPlayerName();
        }

        int currentHP = PlayerStatus.Instance.currentHP;
        int maxHP = PlayerStatus.Instance.maxHP;

        if(playerHPText != null)
        {
            playerHPText.text = $"{currentHP} / {maxHP}";
        }

        if(playerHPBar != null)
        {
            playerHPBar.maxValue = maxHP;
            playerHPBar.value = currentHP;
        }

        if(playerIconImage != null)
        {
            CompanionStatus playerCompanion =
                CompanionManager.Instance != null
                    ? CompanionManager.Instance.GetPlayerStatus()
                    : null;

            Sprite playerIcon =
                playerCompanion != null &&
                playerCompanion.Data != null
                    ? playerCompanion.Data.icon
                    : null;

            playerIconImage.sprite = playerIcon;
            playerIconImage.enabled = playerIcon != null;
        }
    }

    private void UpdateEquipmentItemStatus(ItemData previewItem)
    {
        if(PlayerStatus.Instance == null)
        {
            Debug.LogWarning("ShopManager: PlayerStatus.Instance が存在しません");
            return;
        }

        if(EquipmentManager.Instance == null)
        {
            Debug.LogWarning("ShopManager: EquipmentManager.Instance が存在しません");
            return;
        }

        int currentAttack = PlayerStatus.Instance.Attack;
        int currentDefense = PlayerStatus.Instance.Defense;
        int currentCharge = PlayerStatus.Instance.Charge;

        if(equipmentAttackText != null)
        {
            equipmentAttackText.text = currentAttack.ToString();
        }

        if(equipmentDefenseText != null)
        {
            equipmentDefenseText.text = currentDefense.ToString();
        }

        if(equipmentChargeText != null)
        {
            equipmentChargeText.text = currentCharge.ToString();
        }

        ItemData currentWeapon =
            EquipmentManager.Instance.GetEquipped(EquipData.EquipType.Weapon);

        if(currentWeaponText != null)
        {
            currentWeaponText.text =
                currentWeapon != null
                    ? currentWeapon.itemName
                    : "素手";
        }

        ItemData currentArmor =
            EquipmentManager.Instance.GetEquipped(EquipData.EquipType.Armor);

        if(currentArmorText != null)
        {
            currentArmorText.text =
                currentArmor != null
                    ? currentArmor.itemName
                    : "普段着";
        }

        var previewBonus =
            EquipmentManager.Instance.CaluculatePreviewStats(previewItem);

        var previewStatus =
            PlayerStatus.Instance.GetPreviewTotalStats(
                previewBonus.atk,
                previewBonus.def,
                previewBonus.chg
            );

        if(previewAttackText != null)
        {
            int diff = previewStatus.atk - currentAttack;

            previewAttackText.text = previewStatus.atk.ToString();
            previewAttackText.color = GetDiffColor(diff);
        }

        if(previewDefenseText != null)
        {
            int diff = previewStatus.def - currentDefense;

            previewDefenseText.text = previewStatus.def.ToString();
            previewDefenseText.color = GetDiffColor(diff);
        }

        if(previewChargeText != null)
        {
            int diff = previewStatus.chg - currentCharge;

            previewChargeText.text = previewStatus.chg.ToString();
            previewChargeText.color = GetDiffColor(diff);
        }
    }

    private Color GetDiffColor(int diff)
    {
        if(diff > 0)
        {
            return Color.yellow;
        }
        else if(diff < 0)
        {
            return Color.red;
        }
        else
        {
            return Color.white;
        }
    }

    void SetItemSelectionNavigetion(bool enable)
    {
        foreach(Transform child in itemListContainer)
        {
            Selectable selectable = child.GetComponent<Selectable>();
            if(selectable == null) continue;

            Navigation nav = selectable.navigation;
            nav.mode = enable ? Navigation.Mode.Explicit : Navigation.Mode.None;
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
        if(itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if(itemDescription != null)
        {
            itemDescription.text = "";
        }

        if(buyConfirmPanel != null)
        {
            buyConfirmPanel.SetActive(false);
        }

        CurrentState = ShopState.ItemSelection;
        SetItemSelectionNavigetion(true);

        UpdateItemSelectionGoldUI();

        if(itemListContainer.childCount > 0 && EventSystem.current != null)
        {
            int index = Mathf.Clamp(lastSelectedIndex, 0, itemListContainer.childCount - 1);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(itemListContainer.GetChild(index).gameObject);
        }
    }

    public void CloseItemSelection()
    {
        if (shopMenuPanel != null)
        {
            shopMenuPanel.SetActive(false);
        }

        if(EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if(selectedItemNameText != null)
        {
            selectedItemNameText.text = "";
        }

        if(selectedItemEffectText != null)
        {
            selectedItemEffectText.text = "";
        }

        SelectedItem = null;

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
