using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractDetector : MonoBehaviour
{
    // 調べることが出来るもの(NPCや宝箱など)をまとめるためのリスト
    private List<GameObject> currentTargets = new List<GameObject>();
    // MainMoveスクリプトを入れるための変数
    private MainMove player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // MainMoveがついているObjectを探索、playerと命名
        player = UnityEngine.Object.FindFirstObjectByType<MainMove>();
    }
    // プレイヤーがInteractボタンを押したときに呼び出される関数
    public void OnInteract()
    {
        if(MessageUI.Instance != null && MessageUI.Instance.IsShowing)
        {
            return;
        }

        // currentTargetsに入っているオブジェクトがある場合
        if (currentTargets.Count > 0)
        {
            // リストの先頭をInteract
            Interact(currentTargets[0]);
        }
    }

    // 実行機能(実行されたオブジェクトはobjとして扱う)
    void Interact(GameObject obj)
    {
        // Object内のDialogueTriggerを取り出す
        DialogueTrigger npc = obj.GetComponent<DialogueTrigger>();
        // もしNPCがいるなら
        if(npc != null)
        {
            if (!npc.CanStart())
            {
                return;
            }
            // 親ObjectのPlayerの位置座標を渡して、NPC側で計算してもらう
            npc.LookAtPlayer(transform.root.position);
            // NPCからjsonファイルを取得
            TextAsset json = npc.GetStoryJson();
            if(json == null)
            {
                return;
            }
            
            Debug.Log(npc.GetNPCName() + "会話を開始します");

            // SetShopNPC関数にShopNPCスクリプトを渡す
            InkManager.Instance.SetShopNPC(npc.GetComponent<ShopNPC>());

            npc.MarkTriggered();

            if(player != null)
            {
                player.isInputLocked = true;
            }

            GameManager.Instance.ChangeState(npc.GetInteractionState());

            if(npc.UseFadeOnEventStart && npc.GetInteractionState() == GameState.Event && SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.RequestEventFade(() =>
                {
                    if (npc.CameraFocusTarget != null)
                    {
                        SceneTransitionManager.Instance.FocusCameraOn(npc.CameraFocusTarget);
                    }

                    InkManager.Instance.StartStory(json);
                });
            }
            else
            {
                InkManager.Instance.StartStory(json);
            }

            return;
        }

        CompanionNPC companionNPC = obj.GetComponent<CompanionNPC>();

        if(companionNPC != null)
        {
            Debug.Log(
                $"仲間NPCとの会話を開始: {companionNPC.gameObject.name}");

            companionNPC.Interact();

            GameManager.Instance.ChangeState(GameState.Dialogue);

            return;
        }

        TreasureChest chest = obj.GetComponent<TreasureChest>();

        if(chest != null)
        {
            chest.Interact();
            return;
        }
    }

    // アクションボックスにオブジェクトが入った時
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // もし初めて話しかける場合
        if (!currentTargets.Contains(collision.gameObject))
        {
            // 対象をリストに追加
            currentTargets.Add(collision.gameObject);
        }
    }

    // アクションボックスがオブジェクトから離れた時
    private void OnTriggerExit2D(Collider2D collision)
    {
        // 対象をリストから削除
        currentTargets.Remove(collision.gameObject);
    }
}
