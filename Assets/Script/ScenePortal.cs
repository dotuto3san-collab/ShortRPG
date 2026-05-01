using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePortal : MonoBehaviour
{
    [Header("移動設定")]
    // Inspectorにて移動先のScene名を入力
    public string targetSceneName;
    // Inspectorにて移動先のSceneのワープ地点座標を入力
    public Vector2 targetPosition;
    // 扉(ScenePortal)に触れたか確認、触れたらフラグを立てる
    public bool IsTransitioning { get; private set; } = false;

    [Header("演出設定")]
    // InspectorにてAnimatorが付いてるFadeImageを参照
    [SerializeField] private Animator fadeAnimator;
    // フェードアウト演出の時間を設定
    private float delayBeforeTime = 0.3f;
    // フェードイン演出の時間を設定
    private float delayAfterTime = 0.3f;

    // ワープの準備を開始
    private void OnTriggerEnter2D(Collider2D other)
    {
        // もしプレイヤーが触れて、かつ移動を開始していない場合に移動準備コルーチン開始
        if (other.CompareTag("Player") && !IsTransitioning)
        {
            // プレイヤーを引数にしてワープするコルーチンを開始
            StartCoroutine(PrepareTransition(other.gameObject));
        }
    }

    
    private IEnumerator PrepareTransition(GameObject playerObj)
    {
        // 移動中フラグを立てる
        IsTransitioning = true;

        GameManager.Instance.SetSceneTransitioning(true);

        // ぶつかってきたObject(Player)からMainMoveスクリプトを探す
        MainMove player = playerObj.GetComponent<MainMove>();
        // 入力ロックをオンにする
        if(player != null) player.isInputLocked = true;

        // 移動を開始する前にも、現在のsceneのFadeImageを捕まえ直す
        FindFadeAnimatorInCurrentScene();

        // 移動前の待機
        if (fadeAnimator != null)
        {
            // GameObjectのFadeImageを有効化
            fadeAnimator.gameObject.SetActive(true);
            // FadeOutAnimationを再生する
            fadeAnimator.Play("FadeOut");
        }

        // 指定時間待機する
        yield return new WaitForSeconds(delayBeforeTime);

        // Objectを次のsceneに持ち越す
        DontDestroyOnLoad(this.gameObject);

        // シーン切り替え
        SceneManager.LoadScene(targetSceneName);
    }

    // Scene移動開始時に実行
    private void OnEnable()
    {
        // シーンロード時にOnSceneLoaded関数を実行するようリストに追加
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Scene移動終了時に実行
    private void OnDisable()
    {
        // シーンロード完了時に関数をリストから削除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 新しいシーンへプレイヤーを転送して、移動ロックの解除の準備
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 現在ワープ中でないなら下記処理を無視
        if (!IsTransitioning) return;

        // 目的のシーンに到着したか確認
        if(scene.name == targetSceneName)
        {
            // 新しいシーンに来たのでFadeImageをまた捕まえ直す
            FindFadeAnimatorInCurrentScene();
            // 新しいシーンにいるプレイヤーを探す
            MainMove player = Object.FindFirstObjectByType<MainMove>();
            if(player != null)
            {
                Debug.Log("Warping player to:"+ targetPosition);
                // 指定の座標にワープ
                player.transform.position = targetPosition;

                // 少しだけ待ってからロック解除(バグ防止)
                StartCoroutine(ReleaseLockCoroutine(player));
            }
        }
    }

    private void FindFadeAnimatorInCurrentScene()
    {
        // 常にアクティブなCanvasを起点に探す方法
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if(canvas != null)
        {
            // FadeImageを探す
            Transform found = canvas.transform.Find("FadeImage");
            // もしFadeImageが見つかった場合
            if(found != null)
            {
                // FadeAnimationを実行
                fadeAnimator = found.GetComponent<Animator>();
            }
        }
    }

    private System.Collections.IEnumerator ReleaseLockCoroutine(MainMove player)
    {
        // FadeInアニメーションの実行
        if (fadeAnimator != null)
        {
            // FadeImageを有効化
            fadeAnimator.gameObject.SetActive(true);
            // FadeInAnimationを実行する
            fadeAnimator.Play("FadeIn");
        }

        // 移動後、一定時間待機
        yield return new WaitForSeconds(delayAfterTime);

        // 遷移フラグを下ろす
        IsTransitioning = false;

        GameManager.Instance.SetSceneTransitioning(false);

        // 操作ロックを解除
        player.isInputLocked = false;

        // FadeImageを無効化する
        fadeAnimator.gameObject.SetActive(false);

        // 役目を終えたので削除
        Destroy(this.gameObject);
    }
}
