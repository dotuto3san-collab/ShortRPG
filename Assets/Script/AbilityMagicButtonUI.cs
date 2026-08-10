using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityMagicButtonUI : MonoBehaviour
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
    }
}
