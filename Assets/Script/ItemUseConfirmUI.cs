using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemUseConfirmUI : MonoBehaviour
{
    public static ItemUseConfirmUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private Slider playerHPBar;

    private Action<bool> callback;
    private GameObject previousSelected;

    void Awake()
    {
        Instance = this;
        root.SetActive(false);

        yesButton.onClick.AddListener(OnYes);
        noButton.onClick.AddListener(OnNo);
    }
    
    public bool IsOpen()
    {
        return root != null && root.activeSelf;
    }

    public void Cancel()
    {
        OnNo();
    }

    public void Show(InventoryItem item, Action<bool> onResult)
    {
        root.SetActive(true);

        callback = onResult;

        if(UnityEngine.EventSystems.EventSystem.current != null)
        {
            previousSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(yesButton.gameObject);
        }

        if(messageText != null)
        {
            if(item.itemData.equipData != null
                && item.itemData.equipData.equipType != EquipData.EquipType.None)
            {
                if (EquipmentManager.Instance != null)
                {
                    var equipType = item.itemData.equipData.equipType;
                    var equipped = EquipmentManager.Instance.GetEquipped(equipType);

                    if (equipped == item.itemData)
                    {
                        messageText.text = $"{item.itemData.itemName}を解除しますか？";
                    }
                    else
                    {
                        messageText.text = $"{item.itemData.itemName}を装備しますか？";
                    }
                }
                else
                {
                    messageText.text = $"{item.itemData.itemName}を装備しますか？";
                }
            }
            else
            {
                messageText.text = $"{item.itemData.itemName}を使用しますか？";
            }
        }

        if(PlayerStatus.Instance != null)
        {
            var ps = PlayerStatus.Instance;

            if(playerNameText != null)
            {
                playerNameText.text = ps.GetPlayerName();
            }

            if(playerHPText != null)
            {
                playerHPText.text = $"{ps.currentHP} / {ps.maxHP}";
            }

            if(playerHPBar != null)
            {
                playerHPBar.maxValue = ps.maxHP;
                playerHPBar.value = ps.currentHP;
            }
        }
    }

    void OnYes()
    {
        root.SetActive(false);
        callback?.Invoke(true);

        if(UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            if(previousSelected != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(previousSelected);
            }
        }
    }

    void OnNo()
    {
        root.SetActive(false);
        callback?.Invoke(false);

        if(UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);

            if(previousSelected != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(previousSelected);
            }
        }
    }
}
