using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BattleTargetUI : MonoBehaviour
{
    public static BattleTargetUI Instance;

    [SerializeField] private List<GameObject> enemyRoots;
    [SerializeField] private List<Image> enemyImages;
    [SerializeField] private List<TextMeshProUGUI> enemyNameTexts;

    private List<Color> defaultColors = new List<Color>();

    private int currentIndex = 0;
    private ItemData pendingItem;
    private bool isFromItem = false;

    private MagicData pendingMagic;
    private bool isFromMagic = false;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);

        defaultColors.Clear();
        foreach (var img in enemyImages)
        {
            defaultColors.Add(img.color);
        }

        foreach (var text in enemyNameTexts)
        {
            text.gameObject.SetActive(false);
        }
    }

    public void SetItem(ItemData item)
    {
        pendingItem = item;
        isFromItem = true;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        int enemyCount = BattleManager.Instance.enemies.Count;

        currentIndex = 0;
        for(int i = 0; i < enemyCount; i++)
        {
            if (!BattleManager.Instance.enemies[i].IsDead())
            {
                currentIndex = i;
                break;
            }
        }

        for(int i = 0; i < enemyNameTexts.Count; i++)
        {
            enemyNameTexts[i].gameObject.SetActive(false);
            enemyNameTexts[i].text = "";
        }

        for (int i = 0; i < enemyImages.Count; i++)
        {
            if(i < enemyCount)
            {
                enemyRoots[i].SetActive(true);

                if (!BattleManager.Instance.enemies[i].IsDead())
                {
                    enemyImages[i].sprite = BattleManager.Instance.enemies[i].data.sprite;

                    enemyImages[i].gameObject.SetActive(true);
                    enemyImages[i].color = defaultColors[i];
                }
                else
                {
                   enemyImages[i].gameObject.SetActive(false);
                }
            }
            else
            {
                enemyRoots[i].SetActive(false);
            }
        }

        if(enemyCount == 0)
        {
            Hide();
            return;
        }

        UpdateVisual();
    }

    public void Hide()
    {
        ResetVisual();

        for(int i = 0; i < enemyNameTexts.Count; i++)
        {
            enemyNameTexts[i].gameObject.SetActive(false);
            enemyNameTexts[i].text = "";
        }

        pendingItem = null;
        pendingMagic = null;
        isFromItem = false;
        isFromMagic = false;

        gameObject.SetActive(false);
    }

    void ResetVisual()
    {
        for (int i = 0; i < enemyImages.Count; i++)
        {
            enemyImages[i].color = defaultColors[i];
        }
    }

    public void SetAttackMode()
    {
        pendingItem = null;
        isFromItem = false;
    }

    public void SetMagic(MagicData magic)
    {
        pendingMagic = magic;
        isFromMagic= true;
    }

    void Update()
    {
        if(!gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int count = BattleManager.Instance.enemies.Count;
            int safety = 0;
            do
            {
                currentIndex = (currentIndex + 1) % count;
                safety++;
            }
            while (BattleManager.Instance.enemies[currentIndex].IsDead() && safety < count);

            UpdateVisual();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int count = BattleManager.Instance.enemies.Count;
            int safety = 0;

            do
            {
                currentIndex--;

                if(currentIndex < 0)
                {
                    currentIndex = count - 1;
                }

                safety++;
            }
            while (BattleManager.Instance.enemies[currentIndex].IsDead() && safety < count);

            UpdateVisual();
        }

        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            var target = BattleManager.Instance.enemies[currentIndex];

            BattleManager.Instance.SetTarget(target);

            if(pendingMagic != null)
            {
                BattleManager.Instance.SetCommand(new MagicCommand(pendingMagic));
                pendingMagic = null;
                isFromMagic = false;
            }
            else if(pendingItem != null)
            {
                BattleManager.Instance.SetCommand(new UseItemCommand(pendingItem));
                pendingItem = null;
                isFromItem = false;
            }
            else
            {
                BattleManager.Instance.SetCommand(new AttackCommand());
            }

            Hide();
        }

        if(Input.GetKeyDown(KeyCode.X) ||
           Input.GetKeyDown(KeyCode.LeftShift) ||
           Input.GetKeyDown(KeyCode.RightShift))
        {
            OnCancel();
        }
    }

    void OnCancel()
    {
        Hide();

        if (isFromItem)
        {
            BattleItemUI.Instance.Show();

            if(BattleHelpLog.Instance != null)
            {
                BattleHelpLog.Instance.Show("アイテムを選択してください");
            }
        }
        else if (isFromMagic)
        {
            BattleMagicUI.Instance.Show();

            if(BattleMagicUI.Instance != null)
            {
                BattleHelpLog.Instance.Show("魔法を選択してください");
            }
        }
        else
        {
            BattleCommandUI.Instance.Show();
        }
    }

    void UpdateVisual()
    {
        int enemyCount = BattleManager.Instance.enemies.Count;

        for (int i = 0; i < enemyImages.Count; i++)
        {
            if (i >= enemyCount) continue;

            Color baseColor = defaultColors[i];

            if (i == currentIndex)
            {
                enemyImages[i].color = baseColor;
            }
            else
            {
                 enemyImages[i].color = baseColor * 0.5f;
            }

            if(i == currentIndex)
            {
                enemyNameTexts[i].gameObject.SetActive(true);
                enemyNameTexts[i].text = BattleManager.Instance.enemies[i].data.unitName;
            }
            else
            {
                enemyNameTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
