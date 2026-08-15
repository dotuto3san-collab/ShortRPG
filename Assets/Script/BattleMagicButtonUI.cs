using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BattleMagicButtonUI : MonoBehaviour, ISelectHandler
{
    [SerializeField] private TextMeshProUGUI nameText;

    private MagicData magic;
    private BattleMagicUI parent;
    private int magicIndex;

    public void Setup(MagicData magic,  BattleMagicUI parent, int index)
    {
        this.magic = magic;
        this.parent = parent;
        this.magicIndex = index;

        nameText.text = magic.magicName;

        var button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    public void ShowFocus()
    {
        if(magic == null)
        {
            return;
        }

        if(BattleMagicUI.Instance != null)
        {
            BattleMagicUI.Instance.OnMagicFocused(magic, magicIndex);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(magic == null)
        {
            return;
        }

        if(BattleMagicUI.Instance != null)
        {
            BattleMagicUI.Instance.OnMagicFocused(magic, magicIndex);
        }
    }

    private void OnClick()
    {
        parent.OnMagicSelected(magic);
    }
}
