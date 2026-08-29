using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuStatusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI expText;

    [Header("キャラクター表示")]
    [SerializeField] private Image characterIcon;

    [Header("仲間表示")]
    [SerializeField] private GameObject[] companionRoots;
    [SerializeField] private Image[] companionIcons;
    [SerializeField] private TextMeshProUGUI[] companionNames;

    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI chargeText;

    [Header("HP表示")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Slider hpBar;

    [Header("EXP表示")]
    [SerializeField] private Slider expBar;

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
            Debug.LogError("PlayerStatus.Instance が存在しません");
            return;
        }

        var ps = PlayerStatus.Instance;

        int current = PlayerStatus.Instance.currentHP;
        int max = PlayerStatus.Instance.maxHP;

        if(nameText != null)
        {
            nameText.text = PlayerStatus.Instance.GetPlayerName();
        }

        if(characterIcon != null)
        {
            characterIcon.sprite = ps.GetPlayerIcon();
        }

        if(levelText != null)
        {
            levelText.text = $"{ps.GetLevel()}";
        }

        if(expText != null)
        {
            expText.text =
                $"{ps.GetCurrentExp()} / {ps.GetRequiredExp()}";
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

        if(expBar != null)
        {
            int currentExp = ps.GetCurrentExp();
            int requiredExp = ps.GetRequiredExp();

            expBar.maxValue = requiredExp;
            expBar.value = currentExp;
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

        UpdateCompanionDisplay();
    }

    private void UpdateCompanionDisplay()
    {
        if(CompanionManager.Instance == null)
        {
            Debug.LogError("[MenuStatusUI] ConpanionManager.Instance が存在しません");
            return;
        }

        var companions = CompanionManager.Instance.GetCompanion();

        for(int i = 0;i < companionRoots.Length; i++)
        {
            int companionIndex = i + 1;

            bool hasCompanion =
                companionIndex < companions.Count &&
                companions[companionIndex] != null;

            if (companionRoots[i] != null)
            {
                companionRoots[i].SetActive(hasCompanion);
            }

            if (!hasCompanion)
            {
                continue;
            }

            CompanionStatus companion = companions[companionIndex];

            if(companionIcons[i] != null)
            {
                companionIcons[i].sprite = companion.Data.icon;
                companionIcons[i].enabled = companion.Data.icon != null;
            }

            if (companionNames[i] != null)
            {
                companionNames[i].text = companion.Data.companionName;
            }
        }
    }
}
