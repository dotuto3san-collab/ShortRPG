using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    // Inspector上でこのNPCに話しかけた際にどのゲーム状態にするかを決める。
    [SerializeField] private GameState interactionState = GameState.Dialogue;
    // Inspector上でInkファイルを読み込み
    [Header("このNPCが話すInkファイル(JSON)")]
    [SerializeField] private TextAsset inkJsonAsset;

    [Header("発生条件")]
    [SerializeField] private bool oneShot = false;
    [SerializeField] private string requiredFlag;

    [Header("永続化設定")]
    [SerializeField] private string persistOnceFlag;

    [Header("演出設定")]
    [SerializeField] private bool useFadeOnEventStart = true;
    [SerializeField] private Transform cameraFocusTarget;

    // Animatorを扱うための変数
    private Animator anim;

    private bool hasTriggered;

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

    public bool UseFadeOnEventStart => useFadeOnEventStart;
    public Transform CameraFocusTarget => cameraFocusTarget;

    public bool CanStart()
    {
        if (hasTriggered && oneShot) return false;

        if(oneShot && !string.IsNullOrEmpty(persistOnceFlag))
        {
            if(StoryStateManager.Instance != null && StoryStateManager.Instance.HasFlag(persistOnceFlag))
            {
                hasTriggered = true;
                return false;
            }
        }

        if (!string.IsNullOrEmpty(requiredFlag))
        {
            if(StoryStateManager.Instance == null || !StoryStateManager.Instance.HasFlag(requiredFlag))
            {
                return false;
            }
        }

        return true;
    }

    public void MarkTriggered()
    {
        hasTriggered = true;

        if(oneShot && !string.IsNullOrEmpty(persistOnceFlag) && StoryStateManager.Instance != null)
        {
            StoryStateManager.Instance.SetFlag(persistOnceFlag);
        }
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

    // NPCの名前を返すための公開メソッド
    public string GetNPCName()
    {
        // 対象のNPCの名前を返す
        return gameObject.name;
    }
}
