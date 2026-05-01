using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    private TextMeshProUGUI moneyText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moneyText = GetComponent<TextMeshProUGUI>();
        // もしmoneyTextがnullなら
        if(moneyText == null)
        {
            Debug.LogError("MoneyUI: TextMeshProUGUIが見つかりません。");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Instance != null && moneyText != null)
        {
            // テキストボックスにmoneyを反映
            moneyText.text = GameManager.Instance.Money.ToString();
        }
    }
}
