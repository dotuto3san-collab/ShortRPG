using UnityEngine;

// アイテムのデータと個数を設定できるようにするための関数
[System.Serializable]
public class InventoryItem
{
    // アイテムのデータを入れるための変数
    public ItemData itemData;
    // アイテムの個数を入れるための変数
    public int amount;

    // アイテムのデータと個数を設定するための関数
    public InventoryItem(ItemData data, int amount)
    {
        // 引数で受け取ったアイテムのデータと個数を変数に入れる
        this.itemData = data;
        // 引数で受け取ったアイテムの個数を変数に入れる
        this.amount = amount;
    }
}
