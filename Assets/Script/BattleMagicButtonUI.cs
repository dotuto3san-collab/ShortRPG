using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleMagicButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    private MagicData magic;
    private BattleMagicUI parent;

    public void Setup(MagicData magic,  BattleMagicUI parent)
    {
        this.magic = magic;
        this.parent = parent;

        nameText.text = magic.magicName;

        var button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        parent.OnMagicSelected(magic);
    }
}
