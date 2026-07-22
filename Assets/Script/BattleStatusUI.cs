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
