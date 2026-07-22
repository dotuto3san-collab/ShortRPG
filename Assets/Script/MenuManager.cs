using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MenuState
{
    Main,
    Item,
    Status,
    Equipment
}
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance {  get; private set; }

    public MenuState CurrentMenuState {get; private set;}

    [Header ("UI設定")]
    // 開くメニューをInspectorにて参照
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject itemListPanel;
    [SerializeField] private GameObject statusPanel;
    // メニューを開いた際に最初に選択されるボタンをInspectorにて参照
    [SerializeField] private GameObject firstSelectedButton;

    [Header ("メニューボタン画像設定")]
    // メニューを開くためのボタンをInspectorにて参照
    [SerializeField] private Image menuButtonImage;
    // Inspectorにて、通常時の画像スプライトを参照
    [SerializeField] private Sprite normalSprite;
    // Inspectorにて、使えない時の画像スプライトを参照
    [SerializeField] private Sprite disabledSprite;

    [Header("装備パネル")]
    [SerializeField] private GameObject equipmentPanel;

    // メニューを開いているかどうかをチェック  
    private bool isMenuOpen = false;
    // 最後に選択されていたメインメニューのボタンを保存する変数
    private GameObject lastSelectedMainButton;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        // インスタンスの宣言
        Instance = this;
        // メニュー画面を無効化(gameが始まった時はまだ表示しない)
        if(menuPanel != null ) menuPanel.SetActive( false );
        if(itemListPanel != null ) itemListPanel.SetActive( false );
        if(statusPanel != null ) statusPanel.SetActive( false );
        if(equipmentPanel != null ) equipmentPanel.SetActive( false );
    }
    // Update is called once per frame
    void Update()
    {
        // ボタンの見た目を変更
        UpdateMenuButtonVisual();

        // もしCTRLが押されたら
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            // メニュー画面を切り替える関数を実行
            ToggleMenu();
        }
        // もしメニュー画面を開いているなら
        if(isMenuOpen)
        {
            if(CurrentMenuState == MenuState.Main && EventSystem.current != null)
            {
                var current = EventSystem.current.currentSelectedGameObject;
                if (current != null)
                {
                    lastSelectedMainButton = current;
                }
            }
            // マウスによる誤操作を検知
            HandleMenuLogic();
        }
    }
    // ボタンの見た目を変更
    private void UpdateMenuButtonVisual()
    {
        // もしメニューボタンが参照されてない場合は下記の処理を無視
        if (menuButtonImage == null) return;

        // 遷移中かあるいは会話中か確認
        bool shouldShowDisabled = !CanOpenMenu();

        // もしメニューを開けないタイミングなら
        if (shouldShowDisabled)
        {
            // メニューが使えない時の画像に切り替える
            menuButtonImage.sprite = disabledSprite;
        }
        // メニューを開けるなら
        else
        {
            // メニューが使えるときの画像に切り替える
            menuButtonImage.sprite = normalSprite;
        }
    }

    // メニュー画面を開けるかどうかを検査
    private bool CanOpenMenu()
    {
        if(GameManager.Instance.IsSceneTransitioning) return false;

        return GameManager.Instance.CurrentState == GameState.Exploring
            || GameManager.Instance.CurrentState == GameState.Menu;
    }

    // メニュー画面を開くための関数
    public void ToggleMenu()
    {
        // 開くときだけはCanOpenMenuをチェックする
        if( !isMenuOpen && !CanOpenMenu())
        {
            // 開けない状態なら、ここで処理を終了
            return;
        }

        // メニュー画面の状態を反転
        isMenuOpen = !isMenuOpen;
        // 画面が開いているなら閉じる、画面が閉じているなら開く(フラグ状態によって変更)
        menuPanel.SetActive( isMenuOpen );

        // もしメニュー画面が開いているなら
        if( isMenuOpen )
        {
            // メニューが開いたので状態変更(移動ロック)
            GameManager.Instance.ChangeState(GameState.Menu);
            SetMenuState(MenuState.Main);
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.Exploring);
            CloseMenu();
        }
    }

    // マウスによる誤操作を対策
    private void HandleMenuLogic()
    {
        // マウスの左クリックを検知
        if (Input.GetMouseButtonDown(0))
        {
            // もしクリックした場所がUIの上でない(画面外をクリックした)なら閉じる
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                ToggleMenu();
                // メニューを閉じたので処理を終了
                return;
            }
        }

        // もしマウス操作などで選択状態が外れたら、強制的に戻す
        if(EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void OpenItemMenu()
    {
        SetMenuState(MenuState.Item);
    }

    public void CloseItemMenu()
    {
        SetMenuState(MenuState.Main);

        if(EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void OpenEquipmentMenu()
    {
        SetMenuState(MenuState.Equipment);
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        menuPanel.SetActive( false );

        GameManager.Instance.ChangeState(GameState.Exploring );

        if(EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        lastSelectedMainButton = null;
    }

    public void SetMenuState(MenuState state)
    {
        CurrentMenuState = state;

        if(itemListPanel != null) itemListPanel.SetActive(false);
        if(statusPanel != null) statusPanel.SetActive(false);
        if(equipmentPanel != null) equipmentPanel.SetActive(false);

        switch (state)
        {
            case MenuState.Main:
                if (EventSystem.current != null)
                {
                    GameObject target = lastSelectedMainButton != null
                        ? lastSelectedMainButton
                        : firstSelectedButton;

                    if(target != null)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                        EventSystem.current.SetSelectedGameObject(target);
                    }
                }
                break;

            case MenuState.Item:
                if(itemListPanel != null) itemListPanel.SetActive(true);
                break;

            case MenuState.Status:
                if (statusPanel != null) statusPanel.SetActive(true);
                break;

            case MenuState.Equipment:
                if(equipmentPanel != null) equipmentPanel.SetActive(true);
                break;
        }
    }

    public void OnClickSave()
    {
        if(SaveManager.Instance != null)
        {
            SaveManager.Instance.OnClickSave();
        }
        else
        {
            Debug.LogError("SaveManager Instance not found");
        }
    }
}
