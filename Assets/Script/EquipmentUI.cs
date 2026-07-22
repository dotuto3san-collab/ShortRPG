using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private Slider playerHPBar;

    void OnEnable()
    {
        RefreshPlayerInfo();
    }

    public void RefreshPlayerInfo()
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
    }
}
