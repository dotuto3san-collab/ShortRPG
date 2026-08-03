using UnityEngine;
using Ink.Runtime;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InkManager : MonoBehaviour
{
    // どこからでもこのInkManagerを呼べるようにする(シングルトン)
    public static InkManager Instance {get ; private set;}
    // choiceManagerがボタンを持っているかどうかで判定
    public bool IsChoosing => story != null && story.currentChoices.Count > 0;

    [Header("参照設定")]
    // 文字を表示したいNameTextを参照する
    [SerializeField] private TextMeshProUGUI nameText;
    // 文字を表示したいテキストボックスを入れる為の枠
    [SerializeField] private TextMeshProUGUI dialogueText;
    // 会話ウィンドウの表示、非表示をするためにどのCanvasを操るかを指定する枠
    [SerializeField] private GameObject dialoguePanel;
    // ボタンに関するクラスへの参照
    [SerializeField] private ChoiceManager choiceManager;
    // 会話ウィンドウを上から下に表示するアニメーションの参照
    [SerializeField] private Animator panelAnimator;
    // 次へ進む三角のアイコンを参照
    [SerializeField] private Animator nextIconAnimator;

    [Header("ショップ連携用")]
    // Inspectorにて会話を始めた際に実行する処理を決める
    public UnityEvent onOpenRequest;

    // Inkの物語の状態を保持する変数
    private Story story;
    // 話しかけたNPCを記憶する
    private ShopNPC currentShopNPC;
    // 現在動いているタイピング演出を覚えるための変数
    private Coroutine typingCoroutine;
    // タイピング中かどうか
    private bool isTyping;
    // Animationが終了したかどうか
    private bool isAnimationFinished = false;
    // 分岐選択の選択直後の選択フラグ
    private bool isInputPostChoiceDelay;

    // Startより早く実行される
    void Awake()
    {
        // Instanceがないなら
        if (Instance == null)
        {
            // InstanceにInstanceを入れる
            Instance = this;
        }
        // Instanceがあるなら
        else
        {
            // このオブジェクトを壊す
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // もしChoiceManagerが存在するなら
        if(choiceManager != null)
        {
            // ChoiceManagerのイベントを登録する
            choiceManager.OnChoiceSelected += OnClickChoice;
        }
    }
    // このスクリプトが破棄される直前に実行
    void OnDestroy()
    {
        // もしChoiceManagerが存在するなら
        if (choiceManager != null)
        {
            // 登録されたChoiceManagerのイベントを解除する
            choiceManager.OnChoiceSelected -= OnClickChoice;
        }
    }
    // ストーリー(Ink)を開始する
    public void StartStory(TextAsset inkJson)
    {
        Debug.Log("Start Story");
        // Animationが終了していないことを宣言
        isAnimationFinished = false;
        // EventSystemが存在するなら
        if (EventSystem.current != null)
        {
            // 現在の選択(メニューボタンなど)を解除して決定キーを会話に集中させる
            EventSystem.current.SetSelectedGameObject(null);
        }

        // storyにinkJsontextファイルを入れる
         story = new Story(inkJson.text);

        story.BindExternalFunction(
            "SetFlag",
            (string flagName) => SetFlag(flagName),
            lookaheadSafe: false
        );

        story.BindExternalFunction(
            "HasFlag",
            (string flagName) => HasFlag(flagName),
            lookaheadSafe: false
        );

        // 会話が始まったので会話ウィンドウを表示
        dialoguePanel.SetActive(true);
        // 三角アイコンが存在するなら
        if(nextIconAnimator != null)
        {
            // 初期状態では三角アイコンは隠しておく
            nextIconAnimator.Play("NextIcon_Hidden");
        }

        // 会話ウィンドウを開くアニメーションを開始
        if (panelAnimator != null)
        {
            // Animatorが入っている場合
            if (panelAnimator.runtimeAnimatorController != null)
            {
                // isOpen時のアニメーションを再生
                panelAnimator.SetBool("isOpen", true);
            }
            else
            {
                Debug.LogWarning($"{panelAnimator.name}にAnimator Controllerが設定されていません");
            }
        }
        // 文を生成する関数
        ContinueStory();
    }

    public void SetFlag(string flagName)
    {
        if(StoryStateManager.Instance == null)
        {
            Debug.LogWarning($"StoryStateManager.Instance is null.");
            return;
        }

        StoryStateManager.Instance.SetFlag(flagName);
    }

    public bool HasFlag(string flagName)
    {
        if(StoryStateManager.Instance == null)
        {
            Debug.LogWarning($"StoryStateManager.Instance is null.");
            return false;
        }

        return StoryStateManager.Instance.HasFlag(flagName);
    }

    // 会話中にどの状態か判定し対応した処理を行う
    public void OnSubmit()
    {
        // 分岐選択直後の選択フラグがあるなら下記処理を無視
        if (isInputPostChoiceDelay) return;

        // 物語が始まっていなければ何もしない
        if (story == null) return;

        // もし分岐選択をしている最中なら下記のプログラムを無視
        if (IsChoosing) return;

        // もしタイピング中にZキーが押されたら
        if (isTyping)
        {
            // そのセリフの全文を表示する関数を呼び出す
            FinishTypingEarly();
        }
        // 会話文に続きがある場合
        else if (story.canContinue)
        {
            // 会話文を続ける関数を呼び出す
            ContinueStory();
        }
        else
        {
            // 物語を終了する関数を呼び出す
            FinishStory();
        }
    }

    // 会話ウィンドウを開くAnimationによって実行される関数
    public void OnPanelOpenComplete()
    {
        // 会話ウィンドウが開き終わったフラグを立てる
        isAnimationFinished = true;
    }

    // 現在の行の全文字列を保持
    private string text = ""; 

    // 会話文を表示する
    void ContinueStory()
    {
        // もしタイピング中なら
        if (typingCoroutine != null)
        {
            // コルーチンを止める
            StopCoroutine(typingCoroutine);
        }
        // もしchoiceManagerが参照されているなら
        if (choiceManager != null)
        {
            // ボタンをまっさらにする
            choiceManager.ClearChoices();
        }
        // Inkから次の1行を取得してtextに格納
        text = story.Continue();
        // 名前表示に関する処理を行う
        HandleTags();
        // =====Debug textに格納した文字をデバッグに表示=====
        Debug.Log(text);
        // コルーチンを開始する
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    private void HandleTags()
    {
        // 現在の行についているタグ(# name:)などを全て取得
        List<string> currentTags = story.currentTags;

        // 名前の欄が存在するなら
        if(nameText != null)
        {
            // 名前を空にして一旦リセット(タグがない行で前の名前が残らないようにする)
            nameText.text = "";
        }

        // 名前タグの文字を取り出していく
        foreach (string tag in currentTags)
        {
            // "name:"名前という形式のタグを探す
            if (tag.StartsWith("name:"))
            {
                // NameTagを"name:"と"(名前)"に分けて、"name:"を消して、Trim()で余白を削除しnameに代入
                string name = tag.Replace("name:","").Trim();
                // 名前の欄が存在するなら
                if(nameText != null)
                {
                    // 先ほどトリミングした名前を表示(InkのnameTagのname:部分は表示されない)
                    nameText.text = name;
                }
            }
            // Ink内に書かれている#tagの内容によって処理を分岐
            switch (tag)
            {
                // #tagがdialogueの場合
                case "dialogue":
                    // ゲーム状態をDialogueに変更する関数を実行
                    GameManager.Instance.ChangeState(GameState.Dialogue);
                    break;

                // #tagがshopの場合
                case "shop":
                    // ShopNPCが存在する場合
                    if(currentShopNPC != null)
                    {
                        // 話しかけたNPCの購入画面を開く
                        currentShopNPC.OnShop();
                    }
                    else
                    {
                        Debug.Log("ShopNPCが設定されていません");
                    }
                    break;

                // #tagがsellの場合
                case "sell":
                    // ShopNPCが存在する場合
                    if(currentShopNPC != null)
                    {
                        // 話しかけたNPCの売却画面を開く
                        currentShopNPC.OnSell();
                    }
                    else
                    {
                        Debug.Log("ShopNPCが設定されていません");
                    }
                    break;
            }
        }
    }

    // 一文字ずつタイピングする(コルーチン)
    IEnumerator TypeText(string line)
    {
        // 新しいセリフが始まった場合、三角のマークを非表示
        if (nextIconAnimator != null) nextIconAnimator.Play("NextIcon_Hidden");
        // まず文字を空にする(new textとかが初期で入った時の対策)
        dialogueText.text = "";
        // タイピング中フラグを立てる
        isTyping = true;
        // すでにisOpenがtrueなら無視
        if (panelAnimator != null && panelAnimator.GetBool("isOpen") && !isAnimationFinished)
        {
            // Animationによって会話ウィンドウが開くまで待機
            while (!isAnimationFinished)
            {
                yield return null;
            }
        }

        // 文字列を一文字ずつに分けて、一文字、二文字と処理を行う
        foreach(char letter in line.ToCharArray())
        {
            // もしstoryがnullになってしまった場合(コルーチン中に会話が終了した場合)
            if(story == null)
            {
                // タイピング中フラグをおろす
                isTyping = false;
                // 強制的にコルーチンを終了し、処理終了
                yield break;
            }
            // 文字をdialogueTextに継ぎ足ししていく
            dialogueText.text += letter;
            // ()内の時間分待った後に次の文字に進む
            yield return new WaitForSeconds(0.05f);
        }

        // タイピング中フラグを下ろす
        isTyping=false;
        // タイピング中でないことを示す
        typingCoroutine = null;

        // 文字表示が終了したのでここで点滅アニメーションを開始する
        if(nextIconAnimator != null && story != null)
        {
            // 選択肢がないときだけ出すように制限
            if(story.currentChoices.Count == 0)
            {
                // 次へ進む三角アイコンの表示と点滅Animationを再生
                nextIconAnimator.Play("NextIcon_Blink");
            }
        }

        // 文字を全部表示した後に分岐があるか確認、表示する関数呼び出し
        DisplayChoices();
    }

    // 画面に選択肢を表示する
    void DisplayChoices()
    {
        // 選択肢が出てくるなら三角アイコンを非表示
        if (nextIconAnimator != null) nextIconAnimator.Play("NextIcon_Hidden");
        // もしInkデータに選択肢データがある場合
        if(story.currentChoices.Count > 0 && choiceManager != null)
        {
            // ChoiceManagerにボタンの生成を要求
            choiceManager.CreateChoices(story.currentChoices);
        }
    }

    // クリック時の処理
    void OnClickChoice(int choiceIndex)
    {
        // Inkに選択結果を伝える
        story.ChooseChoiceIndex(choiceIndex); 
        // ボタンを消す
        if (choiceManager != null) choiceManager.ClearChoices();

        // 入力無視フラグを立てる関数を呼び出す
        StartCoroutine(ChoiceInputDelayRoutine());

        // 次の文を表示
        ContinueStory();
    }

    // 連打した場合でも大丈夫なように一部入力を無視する
    IEnumerator ChoiceInputDelayRoutine()
    {
        // ここでフラグを立てる
        isInputPostChoiceDelay = true;

        // 0.2秒ほど入力を無視する
        yield return new WaitForSeconds(0.2f);
        // 入力無視フラグを解除
        isInputPostChoiceDelay = false;
    }

    // 会話中のShopNPCを登録
    public void SetShopNPC(ShopNPC npc)
    {
        // currentShopNPCに会話中のNPCを登録
        currentShopNPC = npc;
    }

    // 会話中にもう一度ZキーorEnterを押すと文字を最後まで一気に表示する
    void FinishTypingEarly()
    {
        // 指定された時間中は実行キーを押しても文字送りスキップを実行しない
        if (isInputPostChoiceDelay) return;

        // もしタイピング中なら
        if(typingCoroutine != null)
        {
            // コルーチンを止める
            StopCoroutine(typingCoroutine);
        }
        // dialogueTextにtextの文をそのまま入れる
        dialogueText.text = text;
        // タイピング中フラグを下ろす
        isTyping = false;
        // タイピング中でないことを示す
        typingCoroutine = null;
        // 飛ばした瞬間に三角アイコンを表示
        if(nextIconAnimator != null && story != null && story.currentChoices.Count == 0)
        {
            // 三角アイコンを表示
            nextIconAnimator.Play("NextIcon_Blink");
        }
        // 文字を全部表示した後に分岐があるか確認、表示する関数呼び出し
        DisplayChoices();
    }

    // ショップの最初の画面に戻るための関数
    public void ReturnToShopMain()
    {
        // もしstoryがnullなら下記処理を無視
        if(story == null) return;
        // Inkのmain地点までワープする
        story.ChoosePathString("main");
        // 文字表示を実行
        ContinueStory();
    }

    //　会話を終了する
    public void FinishStory()
    {
        // =====Debug 物語を終了したことを告知=====
        Debug.Log("物語は終了しました");
        // コルーチンが動いている場合。
        if(typingCoroutine != null)
        {
            // コルーチンを止める
            StopCoroutine(typingCoroutine );
            // コルーチンをnullにする
            typingCoroutine = null;
        }
        // panelAnimatorが存在する場合
        if(panelAnimator != null)
        {
            // 物語終了時にAnimationを閉じる
            panelAnimator.SetBool("isOpen", false);
        }
        // 物語終了時に一番最初のAnimation終了フラグを閉じる
        isAnimationFinished = false;
        // 会話ウィンドウを非表示にする
        dialoguePanel.SetActive(false);
        // storyに入っていたjsonファイルを空にする
        story = null;
        // 会話終了時に再度会話を始めないように0.1秒間をあけて操作ロックを解除
        Invoke("UnlockPlayer", 0.1f);
    }

    // プレイヤーの入力ロックを解除する
    void UnlockPlayer()
    {    
        // ここでプレイヤーの入力ロックを解除する
        MainMove player = UnityEngine.Object.FindFirstObjectByType<MainMove>();
        if (player != null)
        {
            // GameManagerで会話モードを解除する
            GameManager.Instance.ChangeState(GameState.Exploring);
        }
    }
}
