using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AbilityMagicButtonUI : MonoBehaviour,ISelectHandler
{
    [SerializeField] private TextMeshProUGUI nameText;

    private MagicData magic;

    public void Setup(MagicData magic)
    {
        this.magic = magic;

        if(nameText != null)
        {
            nameText.text = magic.magicName;
        }

        Button button = GetComponent<Button>();

        if(button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(magic == null)
        {
            return;
        }

        if(AbilityUI.Instance != null)
        {
            AbilityUI.Instance.OnMagicFocused(magic);
        }
    }

    private void OnClick()
    {
        if(magic == null)
        {
            Debug.LogError("Magic is null");
            return;
        }

        if(AbilityUI.Instance != null)
        {
            AbilityUI.Instance.OnMagicSelected(magic);
        }
    }
}
