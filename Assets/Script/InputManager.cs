using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private bool ignoreSubmit;

    private float inputRepeatDelay = 0.4f;
    private float inputRepeatRate = 0.075f;
    private float nextInputTime = 0f;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Update is called once per frame
    void Update()
    {
        // 決定ボタン (Z,Enter)
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return))
        {
            HandleSubmit();
        }
        // キャンセルボタン (X,SHIFT)
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            Debug.Log("Cancel");
            HandleCancel();
        }
        if(GameManager.Instance.CurrentState == GameState.Shop)
        {
            if(ShopManager.Instance != null)
            {
                ShopManager.Instance.HandleInput();
            }

            var sellUI = ShopManager.Instance.GetSellShopUI();
            if (sellUI != null && sellUI.IsConfirmOpen())
            {
                HandleRepeatKey(KeyCode.UpArrow, +1, sellUI);
                HandleRepeatKey(KeyCode.DownArrow, -1, sellUI);
                HandleRepeatKey(KeyCode.RightArrow, +10, sellUI);
                HandleRepeatKey(KeyCode.LeftArrow, -10, sellUI);
            }

            void HandleRepeatKey(KeyCode key,int delta, SellShopUI sellUI)
            {
                if(Input.GetKeyDown(key))
                {
                    sellUI.ChangeAmount(delta);
                    nextInputTime = Time.time + inputRepeatDelay;
                }
                else if(Input.GetKey(key) && Time.time >= nextInputTime)
                {
                    sellUI.ChangeAmount(delta);
                    nextInputTime = Time.time + inputRepeatRate;
                }
            }
        }
    }
    public void IgnoreNextSubmit()
    {
        ignoreSubmit = true;
    }

    private void HandleSubmit()
    {
        if (ignoreSubmit)
        {
            Debug.Log("Submit ignored due to ignoreSubmit flag.");
            ignoreSubmit = false;
            return;
        }

        bool processedUI = false;

        if(EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;

            if(selected != null)
            {
                ExecuteEvents.Execute(
                    selected,
                    new BaseEventData(EventSystem.current),
                    ExecuteEvents.submitHandler
                );

                processedUI = true;
            }
        }

        if (processedUI)
        {
            return;
        }

        GameState state = GameManager.Instance.CurrentState;
        Debug.Log("決定キーが押されました。現在の状態:"+ state);

        switch(state)
        {
            case GameState.Exploring:
                InteractDetector detector = Object.FindFirstObjectByType<InteractDetector>();
                if (detector == null) Debug.LogError("InteractDetectorが見つかりません！");
                if (detector != null) detector.OnInteract();
                break;

            case GameState.Dialogue:
                if (InkManager.Instance == null) Debug.LogError("InkManagerのInstanceがありません！");
                InkManager.Instance.OnSubmit();
                break;

            case GameState.Shop:
                if (processedUI) return;

                Debug.Log("ShopState: " + ShopManager.Instance.CurrentState);
                Debug.Log("SelectedItem: " + ShopManager.Instance.SelectedItem);
                Debug.Log("IsChoosing: " + InkManager.Instance.IsChoosing);

                break;
        }
    }

    void HandleCancel()
    {
        GameState state = GameManager.Instance.CurrentState;
        Debug.Log($"Cancel pressed. GameState: {state}");

        switch (state)
        {
            case GameState.Menu:
                if (ShopManager.Instance == null) return;
                switch (MenuManager.Instance.CurrentMenuState)
                {
                    case MenuState.Item:
                        MenuManager.Instance.CloseItemMenu();
                        return;

                    case MenuState.Main:
                        MenuManager.Instance.CloseMenu();
                        return;

                    case MenuState.Status:
                        MenuManager.Instance.SetMenuState(MenuState.Main);
                        return;
                }
                break;

            case GameState.Shop:
                if (ShopManager.Instance != null)
                {
                    var sellUI = ShopManager.Instance.GetSellShopUI();
                    if (sellUI != null && sellUI.IsOpen())
                    {
                        if (sellUI.IsConfirmOpen())
                        {
                            sellUI.CloseConfirm();
                        }
                        else
                        {
                            ShopManager.Instance.CloseSellShop();
                        }

                        ignoreSubmit = false;
                        return;
                    }
                }
                switch (ShopManager.Instance.CurrentState)
                {
                    
                    case ShopManager.ShopState.BuyConfirm:
                        ShopManager.Instance.CloseBuyConfirm();
                        break;

                    case ShopManager.ShopState.ItemSelection:
                        ShopManager.Instance.CloseItemSelection();
                        break;

                    case ShopManager.ShopState.ActionSelect:
                        ShopManager.Instance.ExitShop();
                        break;
                
                }

                ignoreSubmit = false;
                return;
        }
        
    }
}
