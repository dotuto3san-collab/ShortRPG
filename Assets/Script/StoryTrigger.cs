using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    [Header("再生するInkファイル")]
    [SerializeField] private TextAsset inkJsonAsset;

    [Header("ゲーム状態")]
    [SerializeField] private GameState interactionState = GameState.Event;

    [Header("発生条件")]
    [SerializeField] private bool oneShot = true;
    [SerializeField] private string requiredFlag;

    [Header("永続化設定")]
    [SerializeField] private string persistOnceFlag;

    [Header("演出設定")]
    [SerializeField] private bool useFadeOnEventStart = true;
    [SerializeField] private Transform cameraFocusTarget;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        MainMove player = other.GetComponent<MainMove>();
        if(player == null) return;
        if (hasTriggered && oneShot) return;

        if(oneShot && !string.IsNullOrEmpty(persistOnceFlag))
        {
            if (StoryStateManager.Instance != null && StoryStateManager.Instance.HasFlag(persistOnceFlag))
            {
                hasTriggered = true;
                return;
            }
        }

        if (!string.IsNullOrEmpty(requiredFlag))
        {
            if(StoryStateManager.Instance == null || !StoryStateManager.Instance.HasFlag(requiredFlag))
            {
                return;
            }
        }

        if (GameManager.Instance.CurrentState != GameState.Exploring) return;

        if (InkManager.Instance == null)
        {
            Debug.LogError("StoryTrigger: InkManager.Instanceが存在しません。");
            return;
        }

        if(inkJsonAsset == null)
        {
            Debug.LogError($"StoryTrigger: Inkファイルが設定されていません。Object = {gameObject.name}");
            return;
        }

        hasTriggered = true;

        if(oneShot && !string.IsNullOrEmpty(persistOnceFlag) && StoryStateManager.Instance != null)
        {
            StoryStateManager.Instance.SetFlag(persistOnceFlag);
        }

        player.isInputLocked = true;

        GameManager.Instance.ChangeState(interactionState);

        if (useFadeOnEventStart && interactionState == GameState.Event && SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.RequestEventFade(() =>
            {
                if(cameraFocusTarget != null)
                {
                    SceneTransitionManager.Instance.FocusCameraOn(cameraFocusTarget);
                }

                InkManager.Instance.StartStory(inkJsonAsset);
            });
        }
        else
        {
            InkManager.Instance.StartStory(inkJsonAsset);
        }
    }
}
