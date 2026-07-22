using UnityEngine;
using UnityEngine.UI;

public class SpecialGaugeUI : MonoBehaviour
{
    public  static SpecialGaugeUI Instance;

    [SerializeField] private Slider slider;

    void Awake()
    {
        Instance = this;
    }

    public void SetGauge(int current, int max)
    {
        if (slider == null) return;

        slider.maxValue = max;
        slider.value = current;
    }
}
