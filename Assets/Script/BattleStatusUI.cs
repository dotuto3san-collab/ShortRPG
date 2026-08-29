using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BattleStatusUI : MonoBehaviour
{
    public static BattleStatusUI Instance;

    [SerializeField] private Slider playerHPBar;
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private List<GameObject> enemyRoots;
    [SerializeField] private List<Image> enemyCharaImages;

    [Header("éÂêlåˆï\é¶")]
    [SerializeField] private Image playerIconImage;

    [Header("íáä‘ï\é¶")]
    [SerializeField] private List<GameObject> companionRoots;
    [SerializeField] private List<Image> companionIconImages;

    void Awake()
    {
        Instance = this;
    }

    public void Init(BattleUnit player, List<BattleUnit> enemies)
    {
        playerHPBar.maxValue = player.GetMaxHP();

        for(int i = 0; i < enemyRoots.Count; i++)
        {
            var root = enemyRoots[i];

            if(i < enemies.Count && enemies[i] != null)
            {
                root.SetActive(true);
            }
            else
            {
                root.SetActive(false);
                continue;
            }

            var slider = root.GetComponentInChildren<Slider>(true);
            if(slider == null)
            {
                Debug.LogError($"Slider missing in enemyRoots[{i}]");
                continue;
            }

            var image = root.GetComponentInChildren<Image>(true);
            if (i < enemyCharaImages.Count && enemyCharaImages[i] != null)
            {
                enemyCharaImages[i].sprite = enemies[i].data.sprite;
                enemyCharaImages[i].gameObject.SetActive(!enemies[i].IsDead());
            }

            if (i < enemies.Count && enemies[i] != null)
            {
                if (!enemies[i].IsDead())
                {
                    slider.gameObject.SetActive(true);

                    slider.maxValue = enemies[i].data.MaxHP;
                    slider.value = enemies[i].GetHP();
                }
                else
                {
                    slider.gameObject.SetActive(false);
                }
            }
            else
            {
                slider.gameObject.SetActive(false);
            }
        }

        UpdateHP(player);

        UpdatePlayerIcon();
        UpdateCompanionDisplay();
    }

    private void UpdatePlayerIcon()
    {
        if(playerIconImage == null)
        {
            return;
        }

        if(CompanionManager.Instance == null)
        {
            playerIconImage.sprite = null;
            playerIconImage.gameObject.SetActive(false);
            return;
        }

        CompanionStatus playerStatus =
            CompanionManager.Instance.GetPlayerStatus();

        if(playerStatus == null ||
           playerStatus.Data == null ||
           playerStatus.Data.icon == null)
        {
            playerIconImage.sprite = null;
            playerIconImage.gameObject.SetActive(false);
            return;
        }

        playerIconImage.sprite = playerStatus.Data.icon;
        playerIconImage.gameObject.SetActive(true);
    }

    private void UpdateCompanionDisplay()
    {
        if(CompanionManager.Instance == null)
        {
            HideAllCompanions();
            return;
        }

        var companions = CompanionManager.Instance.Companions;

        for(int i = 0; i < companionRoots.Count; i++)
        {
            int companionIndex = i + 1;

            if(companionIndex >=  companions.Count ||
               companions[companionIndex] == null)
            {
                companionRoots[i].SetActive(false);

                if(i < companionIconImages.Count &&
                   companionIconImages[i] != null)
                {
                    companionIconImages[i].sprite = null;
                }

                continue;
            }

            CompanionStatus companion = companions[companionIndex];

            if(companion.Data == null)
            {
                companionRoots[i].SetActive(false);

                if(i < companionIconImages.Count &&
                   companionIconImages[i] != null)
                {
                    companionIconImages[i].sprite = null;
                }

                continue;
            }

            companionRoots[i].SetActive(true);

            if(i < companionIconImages.Count &&
               companionRoots[i] != null)
            {
                companionIconImages[i].sprite = companion.Data.icon;
            }
        }
    }

    private void HideAllCompanions()
    {
        for(int i = 0;i < companionRoots.Count; i++)
        {
            if(companionRoots[i] != null)
            {
                companionRoots[i].SetActive(false);
            }

            if(i < companionIconImages.Count &&
               companionIconImages[i] != null)
            {
                companionIconImages[i].sprite = null;
            }
        }
    }

    public void UpdateHP(BattleUnit unit)
    {
        if (unit == BattleManager.Instance.player)
        {
            playerHPBar.value = unit.GetHP();
            playerHPText.text = $"{unit.GetHP()} / {unit.GetMaxHP()}";

            if(playerNameText != null)
            {
                playerNameText.text = unit.GetUnitName();
            }
        }
        else
        {
            int index = BattleManager.Instance.enemies.IndexOf(unit);
            
            if(index >= 0 && index < enemyRoots.Count)
            {
                var root = enemyRoots[index];

                var slider = root.GetComponentInChildren<Slider>(true);
                if(slider != null)
                {
                    slider.value = unit.GetHP();
                }

                if(index < enemyCharaImages.Count && enemyCharaImages[index] != null)
                {
                    enemyCharaImages[index].gameObject.SetActive(!unit.IsDead());
                }
            }
        }
    }
}
