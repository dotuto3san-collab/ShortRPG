using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    // Inspector上でこのNPCに話しかけた際にどのゲーム状態にするかを決める。
    [SerializeField] private GameState interactionState = GameState.Dialogue;
    // Inspector上でInkファイルを読み込み
    [Header("このNPCが話すInkファイル(JSON)")]
    [SerializeField] private TextAsset inkJsonAsset;

    // Animatorを扱うための変数
    private Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ObjectからAnimatorを確保
        anim = GetComponent<Animator>();   
    }
    public GameState GetInteractionState()
    {
        // Inspectorで実際に指定したゲーム状態を返す
        return interactionState;
    }

    // プレイヤーの位置を確認して、プレイヤーの方向へ向く
    public void LookAtPlayer(Vector2 playerPosition)
    {
        // ObjectにAnimatorがついてない場合
        if (anim == null) return;

        Debug.Log("受け取ったプレイヤー座標:" + playerPosition);
        Debug.Log("NPC自身の座標:" + (Vector2)transform.position);

        // 自分の位置からプレイヤーの位置への方向を計算
        Vector2 direction = playerPosition - (Vector2)transform.position;

        // Animatorのパラメーターに値をセット
        anim.SetFloat("Horizontal", direction.normalized.x);
        anim.SetFloat("Vertical",direction.normalized.y);
    }

    // Inkファイルを渡すための公開メソッド
    public TextAsset GetStoryJson()
    {
        // Inspectorで読み込んだinkJsonAssetを返す
        return inkJsonAsset;
    }

    public string GetNPCName()
    {
        // 対象のNPCの名前を返す
        return gameObject.name;
    }
}
