using System.Collections;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("宝箱の画像")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openedSprite;
    [SerializeField] private Sprite halfOpenedSprite;

    [Header("中身")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int itemAmount = 1;

    private SpriteRenderer spriteRenderer;

    private bool isOpened;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(spriteRenderer == null)
        {
            Debug.LogWarning(
                $"TreasureChest: SpriteRendererがありません。Object = {gameObject.name}");
        }
    }

    public void Interact()
    {
        if (isOpened)
        {
            Debug.Log("この宝箱は既に開いています。");
            StartCoroutine(
                MessageUI.Instance.ShowMessage("この宝箱は空だ"));
            return;
        }

        StartCoroutine(OpenRoutine());
    }

    public IEnumerator OpenRoutine()
    {
        if (isOpened)
        {
            yield break;
        }

        if(itemData == null)
        {
            Debug.LogError(
                $"TreasureChest: 中身のItemDataが設定されていません。Object = {gameObject.name}");
            yield break;
        }

        if(itemAmount <= 0)
        {
            Debug.LogError(
                $"TreasureChest: itemAmountが不正です。Amount = {itemAmount}");
            yield break;
        }
        
        if(spriteRenderer != null && openedSprite != null)
        {
            spriteRenderer.sprite = openedSprite;
        }

        int remain = InventoryManager.Instance.GetRemainingCapacity(itemData);

        if(remain < itemAmount)
        {
            Debug.Log($"{itemData.itemName}を{itemAmount}個手に入れようとした");
            Debug.Log($"しかし、これ以上持てないので宝箱に戻した");

            yield return MessageUI.Instance.ShowMessage(
                $"{itemData.itemName}を{itemAmount}個手に入れようとした");

            yield return MessageUI.Instance.ShowMessage(
                $"しかし、これ以上持てないので宝箱に戻した");

            if(spriteRenderer != null && closedSprite != null)
            {
                spriteRenderer.sprite = halfOpenedSprite;
            }

            yield break;
        }

        InventoryManager.Instance.AddItem(itemData, itemAmount);

        isOpened = true;

        yield return MessageUI.Instance.ShowMessage(
            $"{itemData.itemName}を{itemAmount}個手に入れた",
            itemData.icon,
            itemData.effectText);

        Debug.Log(
            $"宝箱を開けました: {gameObject.name} / {itemData.itemName} ×{itemAmount}");
    }

    public bool IsOpened()
    {
        return isOpened;
    }
}
