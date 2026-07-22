using UnityEngine;
using System.Collections;

public class UseItemCommand : IBattleCommand
{
    private ItemData item;

    public UseItemCommand(ItemData item)
    {
        this.item = item;
    }

    public IEnumerator Execute(BattleUnit user, BattleUnit target)
    {
        if (item == null)
        {
            Debug.LogError("Item is null.");
            yield break;
        }

        yield return BattleLogUI.Instance.ShowLogAndWait($"{user.data.unitName}‚Í{item.itemName}‚ðŽg‚Á‚½!");

        if(item.useEffect == null)
        {
            Debug.LogError("ItemEffect is null");
            yield break;
        }

        yield return item.useEffect.Apply(user, target);

        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(item, 1);
        }
    }
}
