using UnityEngine;

// 右クリックから生成できるようにする
[CreateAssetMenu(fileName = "NewItem", menuName = "RPG/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] public string itemId;
    // アイテムの名前を入力
    public string itemName;
    [TextArea]
    // 説明文を記入
    public string description;
    // アイテムのアイコンをSprite形式で読み込み
    public Sprite icon;

    [Header("価格設定")]
    // アイテムの値段を入力
    public int buyPrice;
    // アイテムの売値を入力
    public int sellPrice;

    [Header("売却設定")]
    public bool canSell = true;
    
    // アイテムのタイプを入力
    public enum ItemType { Item, Weapon, Armor, Accessor, Magic}
    [Header("種類とレアリティ")]
    public ItemType type;
    
    // レアリティの設定
    public enum Rarity
    {
        N,
        R,
        SR,
        SSR,
        UR,
        STAR,
        DIAMOND,
        MASTER,
        LEGEND,
        Never
    }
    public Rarity rarity;
}
