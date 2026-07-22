using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    // どこからでもアクセスできるInputManagerのインスタンス
    public static InputManager Instance { get; private set; }

    // Shopにて数量を連続で変更する際の開始の待ち時間
    private float inputRepeatDelay = 0.4f;
    // Shopにて数量を何秒間隔で変更するか
    private float inputRepeatRate = 0.075f;
    // 次の入力を受け付ける時間を記録する変数
    private float nextInputTime = 0f;

    private float navNextInputTime = 0f;

    // 最も早く実行される関数
    void Awake()
    {
        // インスタンスがない場合
        if(Instance == null)
        {
            // このInputManagerをインスタンスとして確立
            Instance = this;
            // シーンを切り替えてもこのInputManagerを保持する
            DontDestroyOnLoad(gameObject);
        }
        // 既にインスタンスが存在する場合
        else
        {
            // このInputManagerは不要なので消す
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        // 決定ボタン (Z,Enter)が押されたとき
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // 決定処理をまとめたプログラムを実行
            HandleSubmit();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            HandleSubmitForEnter();
        }
        // キャンセルボタン (X,SHIFT)が押されたとき
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            Debug.Log("Cancel");
            // キャンセル処理をまとめたプログラムを実行
            HandleCancel();
        }
        
        // もしShopが開いているなら
        if(GameManager.Instance.CurrentState == GameState.Shop)
        {
            // ShopManagerのInstanceが存在するなら
            if (ShopManager.Instance != null)
            {
                // ShopManagerの入力処理を実行
                ShopManager.Instance.HandleInput();
            }

            // もしShopManagerが存在しない場合は無視する
            if (ShopManager.Instance == null) return;
            // 売却パネルを取得する
            var sellUI = ShopManager.Instance.GetSellShopUI();
            // 売却パネルが存在し、かつ売却の確認画面が開いているなら
            if (sellUI != null && sellUI.IsConfirmOpen())
            {
                // 上キーを押したら数量を+1足す
                HandleRepeatKey(KeyCode.UpArrow, +1, sellUI);
                // 下キーを押したら数量を-1引く
                HandleRepeatKey(KeyCode.DownArrow, -1, sellUI);
                // 右キーを押したら数量を+10足す
                HandleRepeatKey(KeyCode.RightArrow, +10, sellUI);
                // 左キーを押したら数量を-10引く
                HandleRepeatKey(KeyCode.LeftArrow, -10, sellUI);
            }

            if (sellUI != null && sellUI.IsOpen() && !sellUI.IsConfirmOpen())
            {
                HandleRepeatNavKeyCommon(KeyCode.UpArrow, -1);
                HandleRepeatNavKeyCommon(KeyCode.DownArrow, +1);
            }

            if (ShopManager.Instance.CurrentState == ShopManager.ShopState.ItemSelection)
            {
                HandleRepeatNavKeyCommon(KeyCode.UpArrow, -1);
                HandleRepeatNavKeyCommon(KeyCode.DownArrow, +1);  
            }
            
            if(!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.sendNavigationEvents = true;
                }
            }

            // リピート入力を処理する関数
            void HandleRepeatKey(KeyCode key,int delta, SellShopUI sellUI)
            {
                // もし指定されたキーが押されたら
                if(Input.GetKeyDown(key))
                {
                    // 数量を変更する
                    sellUI.ChangeAmount(delta);
                    // 長押しで数量を変更するための入力受付時間を設定する
                    nextInputTime = Time.time + inputRepeatDelay;
                }
                // もし指定されたキーが押されていて、かつ現在の時間が次の入力受付時間を過ぎていたら
                else if (Input.GetKey(key) && Time.time >= nextInputTime)
                {
                    var current = EventSystem.current.currentSelectedGameObject;
                    if(current != null)
                    {
                        int index = current.transform.GetSiblingIndex();
                        int count = current.transform.parent.childCount;

                        if((key == KeyCode.UpArrow && index == 0) ||
                            (key == KeyCode.DownArrow && index == count - 1))
                        {
                            return;
                        }
                    }
                    // 数量を変更する
                    sellUI.ChangeAmount(delta);
                    // リピート間隔の時間を設定する
                    nextInputTime = Time.time + inputRepeatRate;
                }
            }
        }

        if(GameManager.Instance.CurrentState == GameState.Menu)
        {
            if (MenuManager.Instance == null) return;

            switch (MenuManager.Instance.CurrentMenuState)
            {
                case MenuState.Item:
                case MenuState.Equipment:
                case MenuState.Main:
                case MenuState.Status:
                    HandleRepeatNavKeyCommon(KeyCode.UpArrow, -1);
                    HandleRepeatNavKeyCommon(KeyCode.DownArrow, +1);
                    break;
            }
            
            if(!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.sendNavigationEvents = true;
                }
            }
        }

        if (GameManager.Instance.CurrentState == GameState.BattleCommand)
        {
            if(BattleItemUI.Instance != null && BattleItemUI.Instance.IsActive())
            {
                HandleRepeatGridNavKey(KeyCode.UpArrow, 0, -1);
                HandleRepeatGridNavKey(KeyCode.DownArrow, 0, +1);
                HandleRepeatGridNavKey(KeyCode.LeftArrow, -1, 0);
                HandleRepeatGridNavKey(KeyCode.RightArrow, +1, 0);
            }

            if(BattleMagicUI.Instance != null && BattleMagicUI.Instance.IsActive())
            {
                HandleRepeatGridNavKey(KeyCode.UpArrow, 0, -1);
                HandleRepeatGridNavKey(KeyCode.DownArrow, 0, +1);
                HandleRepeatGridNavKey(KeyCode.LeftArrow, -1, 0);
                HandleRepeatGridNavKey(KeyCode.RightArrow, +1, 0);
            }

            if(!Input.GetKey(KeyCode.UpArrow) &&
               !Input.GetKey(KeyCode.DownArrow) &&
               !Input.GetKey(KeyCode.LeftArrow) &&
               !Input.GetKey(KeyCode.RightArrow))
            {
                if(EventSystem.current != null)
                {
                    EventSystem.current.sendNavigationEvents = true;
                }
            }

            if (BattleCommandUI.Instance != null && BattleCommandUI.Instance.IsVisible())
            {
                HandleRepeatNavKeyCommon(KeyCode.UpArrow, -1);
                HandleRepeatNavKeyCommon(KeyCode.DownArrow, +1);
            }

            if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) &&
                !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.sendNavigationEvents = true;
                }
            }
        }
    }

    // 決定ボタンが押されたときの処理をまとめた関数
    private void HandleSubmit()
    {
        // ボタン入力を処理したかどうかを記録する変数
        bool processedUI = false;

        // EventSystemが存在する場合
        if (EventSystem.current != null)
        {
            // 現在選択されているUI要素を取得する
            GameObject selected = EventSystem.current.currentSelectedGameObject;

            // もし選択されているUI要素がある場合
            if (selected != null)
            {
                // そのUI要素にSubmitイベントを送る
                ExecuteEvents.Execute(
                    // イベントを送る対象のUI要素
                    selected,
                    // 送信するイベントデータ
                    new BaseEventData(EventSystem.current),
                    // 呼び出すイベント関数
                    ExecuteEvents.submitHandler
                );
                // UIのSubmitイベントを処理したことを記録する
                processedUI = true;
            }
        }

        // もしUIのSubmitイベントを処理したら
        if (processedUI)
        {
            // Submit処理を無視する
            return;
        }

        // 現在のゲーム状態を取得する
        GameState state = GameManager.Instance.CurrentState;
        Debug.Log("決定キーが押されました。現在の状態:"+ state);

        // ゲーム状態に応じた処理を行う
        switch (state)
        {
            // 探索状態のとき
            case GameState.Exploring:
                // InteractDetectorをシーンの全体から探す
                InteractDetector detector = Object.FindFirstObjectByType<InteractDetector>();
                // もしInteractDetectorが見つからなかったらエラーログを出す
                if (detector == null) Debug.LogError("InteractDetectorが見つかりません！");
                // もしInteractDetectorが見つかったら、InteractDetectorの実行ボタン関数を呼び出す
                if (detector != null) detector.OnInteract();
                break;

            // 会話状態のとき
            case GameState.Dialogue:
                // InkManagerのInstanceがない場合はエラーログを出す
                if (InkManager.Instance == null) Debug.LogError("InkManagerのInstanceがありません！");
                // InkManagerのSubmit関数を呼び出す
                InkManager.Instance.OnSubmit();
                break;

            // ショップ状態のとき
            case GameState.Shop:
                // もしUIのSubmitイベントを処理していたら、下記処理を無視する
                if (processedUI) return;

                Debug.Log("ShopState: " + ShopManager.Instance.CurrentState);
                Debug.Log("SelectedItem: " + ShopManager.Instance.SelectedItem);
                Debug.Log("IsChoosing: " + InkManager.Instance.IsChoosing);

                break;
        }
    }

    private void HandleSubmitForEnter()
    {
        // EventSystemが存在する場合
        if (EventSystem.current != null)
        {
            // 選択しているボタンを取得
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            //　もしボタンが選択されているなら
            if (selected != null)
            {
                // EventSystemに任せる
                return;
            }
        }

        // 現在のゲーム状態を取得
        GameState state = GameManager.Instance.CurrentState;

        // 現在のゲーム状態に合わせて処理を変更
        switch (state)
        {
            // ゲーム状態が自由行動の場合
            case GameState.Exploring:
                // InteractDetectorをフレームの中から探す
                InteractDetector detector = Object.FindFirstObjectByType<InteractDetector>();
                // InteractDetectorが存在するなら、決定機能を実行する
                if (detector != null) detector.OnInteract();
                break;

            // ゲーム状態が会話中の場合
            case GameState.Dialogue:
                // InkManagerが存在するなら、Ink機能の決定機能を実行する
                if (InkManager.Instance != null) InkManager.Instance.OnSubmit();
                break;
        }
    }

    // キャンセルボタンが押されたときの処理をまとめた関数
    void HandleCancel()
    {
        if(ItemUseConfirmUI.Instance != null && ItemUseConfirmUI.Instance.IsOpen())
        {
            ItemUseConfirmUI.Instance.Cancel();
            return;
        }

        // 現在のゲーム状態を取得する
        GameState state = GameManager.Instance.CurrentState;
        Debug.Log($"Cancel pressed. GameState: {state}");

        // ゲーム状態に応じた処理を行う
        switch (state)
        {
            // 現在のゲーム状態がメニューのとき
            case GameState.Menu:
                // もしMenuManagerのInstanceがない場合は無視する
                if (MenuManager.Instance == null) return;
                // MenuManagerの現在のメニュー状態に応じた処理を行う
                switch (MenuManager.Instance.CurrentMenuState)
                {
                    // アイテムメニューのとき
                    case MenuState.Item:
                        // アイテムメニューを閉じる
                        MenuManager.Instance.CloseItemMenu();
                        return;

                    // メインメニューのとき
                    case MenuState.Main:
                        // メインメニュー画面を閉じる
                        MenuManager.Instance.CloseMenu();
                        return;

                    // ステータスメニューのときはメインメニューに戻る
                    case MenuState.Status:
                        // ステータスメニューを閉じてメインメニューに戻る
                        MenuManager.Instance.SetMenuState(MenuState.Main);
                        return;

                    case MenuState.Equipment:
                        MenuManager.Instance.SetMenuState(MenuState.Main);
                        return;
                }
                break;

            // 現在のゲーム状態がショップのとき
            case GameState.Shop:
                // もしShopManagerのInstanceが存在するなら
                if (ShopManager.Instance != null)
                {
                    // 売却パネルを取得する
                    var sellUI = ShopManager.Instance.GetSellShopUI();
                    // 売却パネルが存在し、かつ売却の確認画面が開いているなら
                    if (sellUI != null && sellUI.IsOpen())
                    {
                        // もし売却の確認画面が開いているなら
                        if (sellUI.IsConfirmOpen())
                        {
                            // 売却の確認画面を閉じる
                            sellUI.CloseConfirm();
                        }
                        else
                        {
                            // 売却パネルを閉じる
                            ShopManager.Instance.CloseSellShop();
                        }
                        return;
                    }
                }
                // ShopManagerの現在のショップ状態に応じた処理を行う
                switch (ShopManager.Instance.CurrentState)
                {
                    // 購入の確認画面のとき
                    case ShopManager.ShopState.BuyConfirm:
                        // 購入の確認画面を閉じる
                        ShopManager.Instance.CloseBuyConfirm();
                        break;

                    // アイテムを選択する画面のとき
                    case ShopManager.ShopState.ItemSelection:
                        // アイテム選択画面を閉じる
                        ShopManager.Instance.CloseItemSelection();
                        break;

                    // 購入売却を選択する画面のとき
                    case ShopManager.ShopState.ActionSelect:
                        // 購入売却選択画面を閉じる
                        ShopManager.Instance.ExitShop();
                        break;
                
                }
                return;
        }
        
    }

    private void HandleRepeatNavKeyCommon(KeyCode key, int direction)
    {
        if(EventSystem.current == null) return;

        if(Input.GetKeyDown(key))
        {
            navNextInputTime = Time.time + inputRepeatDelay;
            return;
        }

        if (Input.GetKey(key))
        {
            EventSystem.current.sendNavigationEvents = false;

            if(Time.time >= navNextInputTime)
            {
                var current = EventSystem.current.currentSelectedGameObject;
                if (current == null) return;

                int index = current.transform.GetSiblingIndex();
                int count = current.transform.parent.childCount;

                if (direction < 0 && index == 0) return;
                if (direction > 0 && index == count - 1) return;

                int next = index + direction;
                var nextObj = current.transform.parent.GetChild(next).gameObject;
                EventSystem.current.SetSelectedGameObject(nextObj);

                navNextInputTime = Time.time + inputRepeatRate;
            }
        }
    }

    private void HandleRepeatGridNavKey(KeyCode key, int xDir, int yDir)
    {
        if (EventSystem.current == null) return;

        const int colume = 3;

        if (Input.GetKeyDown(key))
        {
            navNextInputTime = Time.time + inputRepeatDelay;
            return;
        }

        if (Input.GetKey(key))
        {
            EventSystem.current.sendNavigationEvents = false;

            if (Time.time >= navNextInputTime)
            {
                var current = EventSystem.current.currentSelectedGameObject;
                if (current == null) return;

                Transform parent = current.transform.parent;
                int index = current.transform.GetSiblingIndex();
                int count = parent.childCount;

                int row = index / colume;
                int col = index % colume;
                int maxRow = (count - 1) / colume;

                int rowStart = row * colume;
                int rowEnd = Mathf.Min(rowStart + colume - 1, count - 1);

                if (yDir != 0)
                {
                    int nextRow = row + yDir;
                    
                    if (nextRow < 0 || nextRow > maxRow) return;

                    int nextIndex = nextRow * colume + col;
                    
                    if (nextIndex >= count) return;

                    EventSystem.current.SetSelectedGameObject(parent.GetChild(nextIndex).gameObject);
                }
                else if (xDir != 0)
                {
                    int nextCol = col + xDir;
                    
                    if (nextCol < 0 || nextCol >= colume) return;

                    int nextIndex = row * colume + nextCol;
                    
                    if (nextIndex >= count || nextIndex < rowStart) return;

                    EventSystem.current.SetSelectedGameObject(parent.GetChild(nextIndex).gameObject);
                }

                navNextInputTime = Time.time + inputRepeatRate;
            }
        }
    }
}
