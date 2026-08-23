using UnityEngine;

public class CompanionNPC : MonoBehaviour
{
    [Header("Ink")]
    [SerializeField] private TextAsset inkJson;

    [Header("仲間データ")]
    [SerializeField] private CompanionData companionData;

    public void Interact()
    {
        if(InkManager.Instance == null)
        {
            Debug.LogError(
                "CompanionNPC: InkManager.Instanceが存在しません。");

            return;
        }

        if(inkJson == null)
        {
            Debug.LogError(
                $"CompanionNPC: InkManager.Instanceが存在しません。");

            return;
        }

        if(companionData == null)
        {
            Debug.LogError(
                $"CompanionNPC: CompanionDataが設定されていません。" +
                $"Object = {gameObject.name}");

            return;
        }

        InkManager.Instance.SetCompanionNPC(this);
        InkManager.Instance.StartStory(inkJson);
    }

    public CompanionData GetCompanionData()
    {
        return companionData;
    }
}
