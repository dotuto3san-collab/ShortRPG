using UnityEngine;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }
    public System.Action OnEquipmentChanged;

    private Dictionary<EquipData.EquipType, ItemData> equipped
        = new Dictionary<EquipData.EquipType, ItemData>();

    [SerializeField] private ItemData defaultWeapon;
    [SerializeField] private ItemData defaultArmor;

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
    }

    void Start()
    {
        if(defaultWeapon != null)
        {
            InventoryManager.Instance.AddItem(defaultWeapon, 1);
            Equip(defaultWeapon);
        }

        if(defaultArmor != null)
        {
            InventoryManager.Instance.AddItem(defaultArmor, 1);
            Equip(defaultArmor);
        }
    }

    public void Equip(ItemData itemData)
    {
        if(itemData == null || itemData.equipData == null)
        {
            Debug.Log("ëïîıÉfÅ[É^Ç»Çµ");
            return;
        }

        var type = itemData.equipData.equipType;

        if (equipped.ContainsKey(type))
        {
            Unequip(type);
        }

        equipped[type] = itemData;

        ApplyAllEquipmentStats();

        OnEquipmentChanged?.Invoke();

        Debug.Log($"{itemData.itemName}Çëïîı");
    }

    public void Unequip(EquipData.EquipType type)
    {
        if (!equipped.ContainsKey(type)) return;

        var item = equipped[type];
        equipped.Remove(type);

        ApplyAllEquipmentStats();

        OnEquipmentChanged?.Invoke();

        Debug.Log($"{item.itemName}Çâèú");
    }

    public ItemData GetEquipped(EquipData.EquipType type)
    {
        equipped.TryGetValue(type, out var item);
        return item;
    }

    private void ApplyAllEquipmentStats()
    {
        if (PlayerStatus.Instance == null) return;

        int totalAttack = 0;
        int totalDefense = 0;
        int totalCharge = 0;

        foreach(var kv in equipped)
        {
            var item = kv.Value;

            if (item == null ||  item.equipData == null) continue;

            totalAttack += item.equipData.attackPower;
            totalDefense += item.equipData.defensePower;
            totalCharge += item.equipData.chargePower;
        }

        PlayerStatus.Instance.SetBonusStats(totalAttack, totalDefense, totalCharge);
    }

    public (int atk, int def, int chg) CaluculatePreviewStats(ItemData previewItem)
    {
        int totalAttack = 0;
        int totalDefense = 0;
        int totalCharge = 0;

        Dictionary<EquipData.EquipType, ItemData> temp
         = new Dictionary<EquipData.EquipType, ItemData>(equipped);

        if (previewItem != null && previewItem.equipData != null)
        {
            var type = previewItem.equipData.equipType;

            if (type != EquipData.EquipType.None)
            {
                temp[type] = previewItem;
            }
        }

        foreach (var kv in temp)
        {
            var item = kv.Value;
            if (item == null || item.equipData == null) continue;

            totalAttack += item.equipData.attackPower;
            totalDefense += item.equipData.defensePower;
            totalCharge += item.equipData.chargePower;
        }

        return (totalAttack, totalDefense, totalCharge);
    }
}
