using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuStatusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI chargeText;

    [Header("HPï\é¶")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Slider hpBar;

    void Start()
    {
        UpdateHPDisplay();
    }

    void OnEnable()
    {
        if (PlayerStatus.Instance != null)
        {
            Debug.Log("OnEnable currentHP: " + PlayerStatus.Instance.currentHP);
            PlayerStatus.Instance.OnStatusChanged -= UpdateHPDisplay;
            PlayerStatus.Instance.OnStatusChanged += UpdateHPDisplay;

            UpdateHPDisplay();
        }
    }

    void OnDisable()
    {
        if(PlayerStatus.Instance != null)
        {
            PlayerStatus.Instance.OnStatusChanged -= UpdateHPDisplay;
        }
    }

    public void UpdateHPDisplay()
    {
        if(PlayerStatus.Instance == null)
        {
            Debug.LogError("PlayerStatus.Instance Ç™ë∂ç›ÇµÇ‹ÇπÇÒ");
            return;
        }

        int current = PlayerStatus.Instance.currentHP;
        int max = PlayerStatus.Instance.maxHP;

        if(nameText != null)
        {
            nameText.text = PlayerStatus.Instance.GetPlayerName();
        }

        if(hpText != null)
        {
            hpText.text = $"{current} / {max}";
        }

        if(hpBar != null)
        {
            hpBar.maxValue = max;
            hpBar.value = current;
        }

        if(PlayerStatus.Instance != null)
        {
            var ps = PlayerStatus.Instance;

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
        }
    }
}
