using UnityEngine;

// 右クリックから生成できるようにする
[CreateAssetMenu(fileName = "NewItem", menuName = "RPG/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("基本情報")]
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
    public bool canSell = true;

    [Header("使用可否")]
    public bool canUseInBattle;
    public bool canUseInField;

    [Header("効果")]
    public ItemEffect useEffect;
    public EquipData equipData;

    [Header("種類とレアリティ")]
    public Rarity rarity;
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
}
