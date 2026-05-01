using UnityEngine;
using UnityEngine.Tilemaps; // タイルマップAssetsを読み取るため
using System.Collections; // コルーチンを使用するために必要

public class MainMove : MonoBehaviour
{
    // 一マス移動する速度
    public float movespeed = 5f;
    // タイルのサイズを計算(1f = 1マス)
    public float tilesize = 1f; 

    // InspectorにてhitBoxが塗られたTilemapのEmptyを読み込み
    [SerializeField] private Tilemap hitBoxTilemap;
    // InspectorにてAnimatorを含んだEmptyを読み込み
    [SerializeField] private Animator animator; 
    // InspectorにてinterectBoxのEmptyを読み込み
    [SerializeField] private Transform interactBox; 
    
    // 現在、一マス間を移動中かどうかを判定
    private bool isMoving; 
    // プレイヤーの入力を制限する
    public bool isInputLocked;
    // Update is called once per frame
    void Update()
    {
        // 移動中の場合や入力ロックが入ってる場合は無視
        if (isMoving || isInputLocked) return; 
        
        // 水平方向の入力を検知、horizontalに結果を代入
        float horizontal = Input.GetAxisRaw("Horizontal");
        //垂直方向の入力を検知、verticalに結果を代入 
        float vertical = Input.GetAxisRaw("Vertical"); 

        // 斜め移動防止
        if (horizontal != 0) vertical = 0;

        // 移動キーが入力されたら
        if (horizontal != 0 || vertical != 0)
        {
            // セットされたAnimatorEmptyに入力されたキーの値を送信、Animator側でAnimationを変更
            animator.SetFloat("MoveX", horizontal);
            animator.SetFloat("MoveY", vertical);

            // アクションボックスの位置を向きに合わせて更新する
            if(interactBox != null)
            {
                // プレイヤーの中心からhorizontalかverticalの方向に一マスずらす
                interactBox.localPosition = new Vector2(horizontal, vertical) * tilesize;
            }

            // direction = (1, 0) なら右、(0, -1) なら下を指す
            Vector2 direction = new Vector2(horizontal, vertical);
            // 現在地に「方向 × マス目サイズ」を足して、目的地の座標を決める
            Vector2 targetPos = (Vector2)transform.position + direction * tilesize;

            //もしCanMoveの関数がTrueならば
            if (CanMove(targetPos))
            {
                // コルーチンと共に移動開始
                StartCoroutine(MovePlayer(targetPos));
            }
        }

        // 移動中かどうかのフラグ更新
        animator.SetBool("IsMoving",isMoving || (horizontal != 0 || vertical != 0));
    }

    // タイルの有無で判定
    bool CanMove(Vector2 targetPos)
    {
        // もしタイルマップが入ってない場合は何もしない(貫通する)
        if (hitBoxTilemap == null) return true;

        // ワールド座標をタイルの格子座標に変換
        Vector3Int cellPosition = hitBoxTilemap.WorldToCell(targetPos);

        // その場所にタイルがある ＝ 移動不可
        return !hitBoxTilemap.HasTile(cellPosition);
    }

    // コルーチンと移動の処理
    IEnumerator MovePlayer(Vector2 endPos)
    {
        // 移動中フラグをオン
        isMoving = true;
        
        // 移動開始座標を記録
        Vector2 startPos = transform.position;
        // 移動処理の進行状況
        float t = 0;

        // 進行状況がMAXになるまでループ処理
        while (t < 1f)
        {
            // フレームごとの経過時間 * あらかじめ設定したmovespeed
            t += Time.deltaTime * movespeed;
            // 実際にstartPos地点からendPos地点までtパーセント分移動させる
            transform.position = Vector2.Lerp(startPos, endPos, t);
            // ここで一旦処理を中断して次のフレームに移行
            yield return null;
        }

        // 誤差をなくすためにendPos地点でピタッと固定
        transform.position = endPos;
        // 移動中フラグを解除
        isMoving = false;
        // 到着時にもう一度IsMovingを更新して確実にIdleへ移行させる
        animator.SetBool("IsMoving", false);
    }
}
