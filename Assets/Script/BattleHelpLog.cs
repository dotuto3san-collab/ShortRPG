using UnityEngine;
using TMPro;

public class BattleHelpLog : MonoBehaviour
{
    public static BattleHelpLog Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI text;

    void Awake()
    {
        if ( Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        root.SetActive(false);
    }

    public void Show(string message)
    {
        root.SetActive(true);
        text.text = message;
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    public void SetMessage(string message)
    {
        text.text = message;
    }

    public void Clear()
    {
        text.text = "";
    }
}
