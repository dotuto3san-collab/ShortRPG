using UnityEngine;
using UnityEngine.UI;

public class SkillBarUnitUI : MonoBehaviour
{
    [SerializeField] private Image[] bars;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color usedColor;

    private int useCount = 0;
    
    public void Init()
    {
        useCount = 0;

        foreach (var bar in bars)
        {
            if(bar == null)
            {
                Debug.LogError("SkillBarUnitUI: bar is null");
                continue;
            }

            bar.gameObject.SetActive(true);
            bar.color = normalColor;
        }
    }

    public void Use()
    {
        if (useCount >= 3) return;

        bars[useCount].color = usedColor;
        useCount++;
    }
}
