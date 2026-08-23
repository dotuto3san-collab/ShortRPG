using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SkillCompanionButtonUI : MonoBehaviour, ISelectHandler
{
    private CompanionStatus companion;
    private SkillUI parent;

    public CompanionStatus Companion => companion;

    public void Setup(
        CompanionStatus companion,
        SkillUI parent)
    {
        this.companion = companion;
        this.parent = parent;

        if(companion == null)
        {
            Debug.LogError(
                "SkillCompanionButtonUI.Setup: companionがnullです。");

            return;
        }

        if(companion.Data == null)
        {
            Debug.LogError(
            "SkillCompanionButtonUI.Setup: companion.Dataがnullです。");

            return;
        }

        Debug.Log(
        $"SkillCompanionButtonUI.Setup: " +
        $"仲間={companion.Data.companionName} / " +
        $"Icon={companion.Data.icon}");

        Image image = GetComponent<Image>();

        if (image == null)
        {
            Debug.LogError(
                "SkillCompanionButtonUI.Setup: " +
                $"Prefab「{gameObject.name}」にImageコンポーネントがありません。");

            return;
        }

        image.sprite = companion.Data.icon;
        image.enabled = true;

        Debug.Log(
            $"SkillCompanionButtonUI.Setup: " +
            $"Image.sprite={image.sprite}");

        Button button = GetComponent<Button>();

        if(button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (companion == null)
        {
            return;
        }

        if (parent != null)
        {
            parent.SelectCompanion(companion);
        }
    }

    private void OnClick()
    {
        if(parent == null)
        {
            return;
        }

        parent.SelectCompanion(companion);
        parent.ConfirmCompanion();
    }
}
