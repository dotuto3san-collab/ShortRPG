using UnityEngine;

// 現在のゲームの状態を表す配列
public enum GameState
{
    Exploring,    // 探検(自由行動)
    Dialogue,     // 会話中
    Event,        // イベント中
    Menu,         // メニュー閲覧中
    Shop,         // 買い物中
    BattleCommand,// コマンド選択中
    BattleExecute // 戦闘実行中
}

public class GameManager : MonoBehaviour
{
    // シングルトンとして確立(変更は子のクラス内のみ)
    public static GameManager Instance { get; private set; }
    // 今のゲーム状態を確立
    public GameState CurrentState { get; private set; }
    // シーンを切り替え中かどうか判定
    public bool IsSceneTransitioning { get; private set; }
    // お金にまつわる変数
    public int Money { get; private set; } = 500;
    // プレイヤーのMainMoveスクリプトを保持する変数
    public MainMove Player { get; private set; }
    // Playerをセットする関数
    public void SetPlayer(MainMove player)
    {
        // Playerに代入
        Player = player;
    }

    // Startより早く、オブジェクトが生成された直後に実行される
    void Awake()
    {
        // 既に存在するなら自分を消す
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // シングルトンを確立
        Instance = this;
        // シーンをまたいで保持
        DontDestroyOnLoad(gameObject);
        // 初期段階では自由に動ける状態にしておく
        ChangeState(GameState.Exploring );
    }
    // 現在のゲームの状態を切り替える関数
    public void ChangeState(GameState state)
    {
        // 現在のゲームの状態を更新
        CurrentState = state;
        Debug.Log("ゲーム状態が変更されました:" + state);

        // 状態に合わせてプレイヤーの動きを制御
        MainMove player = Player;
        // Playerが存在するなら
        if (player != null)
        {
            // ゲーム状態が自由行動じゃない限りメニューをロックする
            bool isLocked = state != GameState.Exploring;
            // PlayerにisLockedを付与する
            player.isInputLocked = isLocked;

            // ロックされたアニメーションも強制停止
            if (isLocked)
            {
                // Playerに付いているAnimatorを手に入れる
                Animator anim = player.GetComponent<Animator>();
                // animが存在するなら
                if(anim != null)
                {
                    // AnimatorのParameterのIsMovingをfalseにする
                    anim.SetBool("IsMoving", false);
                }
            }
        }
    }

    // シーン切り替えフラグの変更関数
    public void SetSceneTransitioning(bool value)
    {
        // 他クラスで指定されたvalueがそのまま代入
        IsSceneTransitioning = value;
    }

    // お金を増やす関数
    public void AddMoney(int amount)
    {
        // 所持金 + 獲得金額
        Money += amount;
        Debug.Log($"お金を手に入れた! 現在の所持金: {Money}");
    }

    // 指定した金額に所持金を変更する関数
    public void SetMoney(int value)
    {
        // 代入された金額をそのまま所持金にする
        Money = value;
    }

    // お金を減らす関数
    public bool SpendMoney(int amount)
    {
        // もし所持金が支払金額よりも多いなら
        if(Money >= amount)
        {
            // 所持金から支払金額分減らす
            Money -= amount;
            Debug.Log($"{amount}円支払った。残りの所持金: {Money}");
            // 支払成功
            return true;
        }
        else
        {
            Debug.Log("お金が足りません");
            // 支払失敗
            return false;
        }
    }
    
    // ショップ画面を開く(購入アイテム選択画面)
    public void OpenShop()
    {
        // ゲーム状態をショップに変更
        ChangeState(GameState.Shop);
        
        if(ShopManager.Instance != null)
        {
            // 購入アイテム選択画面を表示するための関数を実行
            ShopManager.Instance.OpenItemSelection();
        }
    }

    // インベントリ画面を開く関数
    public void OpenInventory()
    {
        Debug.Log("OpenInventory PlayerStatus: " + (PlayerStatus.Instance == null ? "NULL" : PlayerStatus.Instance.currentHP.ToString()));
        // ゲームステータスをMenuに変更する
        ChangeState(GameState.Menu);
        // MenuManagerのInstanceがあるなら
        if(MenuManager.Instance != null)
        {
            // アイテムメニューを開く関数を実行
            MenuManager.Instance.OpenItemMenu();
        }
    }

    // セーブ可能かどうかを返す関数
    public bool CanSave()
    {
        // ゲーム状態が自由行動かメニューのときにセーブ可能
        return CurrentState == GameState.Exploring ||
            CurrentState == GameState.Menu;
    }
}
