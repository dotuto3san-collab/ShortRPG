using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EquipmentStatusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private Slider playerHPBar;

    [SerializeField] private TextMeshProUGUI weaponText;
    [SerializeField] private TextMeshProUGUI armorText;

    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI chargeText;

    [SerializeField] private TextMeshProUGUI attackPreviewText;
    [SerializeField] private TextMeshProUGUI defensePreviewText;
    [SerializeField] private TextMeshProUGUI chargePreviewText;

    private void TrySubscride()
    {
        if(PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.OnStatusChanged -= Refresh;
            PlayerStatus.Instance.OnStatusChanged += Refresh;
        }
    }

    void OnEnable()
    {
        TrySubscride();

        if(EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged += Refresh;
        }

        StartCoroutine(WaitAndRefresh());
    }

    void OnDisable()
    {
        if(PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.OnStatusChanged -= Refresh;
        }

        if(EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= Refresh;
        }
    }

    private IEnumerator WaitAndRefresh()
    {
        yield return null;

        Refresh();
    }

    public void Refresh()
    {
        if(PlayerStatus.Instance == null)
        {
            Debug.LogError("PlayerStatus Instance not found");
            return;
        }

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

        if(attackText != null)
        {
            attackText.text = ps.Attack.ToString();
        }

        if(defenseText != null)
        {
            defenseText.text = ps.Defense.ToString();
        }

        if(chargeText != null)
        {
            chargeText.text = ps.Charge.ToString();
        }

        if (attackPreviewText != null)
        {
            attackPreviewText.text = "";
            attackPreviewText.color = Color.white;
        }

        if (defensePreviewText != null)
        {
            defensePreviewText.text = "";
            defensePreviewText.color = Color.white;
        }

        if (chargePreviewText != null)
        {
            chargePreviewText.text = "";
            chargePreviewText.color = Color.white;
        }

        if (EquipmentManager.Instance == null)
        {
            Debug.LogWarning("EquipmentManager not found");
            return;
        }

        var weapon = EquipmentManager.Instance.GetEquipped(EquipData.EquipType.Weapon);
        var armor = EquipmentManager.Instance.GetEquipped(EquipData.EquipType.Armor);

        if(weaponText != null)
        {
            weaponText.text = weapon != null ? weapon.itemName : "‘fŽè";
        }

        if(armorText != null)
        {
            armorText.text = armor != null ? armor.itemName : "•’i’…";
        }
    }

    public void ShowPreview(ItemData previewItem)
    {
        if (PlayerStatus.Instance == null || EquipmentManager.Instance == null) return;

        var ps = PlayerStatus.Instance;

        int currentAtk = ps.Attack;
        int currentDef = ps.Defense;
        int currentChg = ps.Charge;

        var previewBonus = EquipmentManager.Instance.CaluculatePreviewStats(previewItem);

        var previewTotal = ps.GetPreviewTotalStats(
            previewBonus.atk,
            previewBonus.def,
            previewBonus.chg
        );

        if (attackPreviewText != null)
        {
            int diff = previewTotal.atk - currentAtk;
            attackPreviewText.text = previewTotal.atk.ToString();
            attackPreviewText.color = GetDiffColor(diff);
        }

        if (defensePreviewText != null)
        {
            int diff = previewTotal.def - currentDef;
            defensePreviewText.text = previewTotal.def.ToString();
            defensePreviewText.color = GetDiffColor(diff);
        }

        if (chargePreviewText != null)
        {
            int diff = previewTotal.chg - currentChg;
            chargePreviewText.text = previewTotal.chg.ToString();
            chargePreviewText.color = GetDiffColor(diff);
        }
    }

    private Color GetDiffColor(int diff)
    {
        if (diff > 0)
        {
            return Color.yellow;
        }
        else if (diff < 0)
        {
            return Color.red;
        }
        else
        {
            return Color.white;
        }
    }

    public void ClearPreview()
    {
        if (attackPreviewText != null) attackPreviewText.text = "";
        if (defensePreviewText != null) defensePreviewText.text = "";
        if (chargePreviewText != null) chargePreviewText.text = "";
    }
}
