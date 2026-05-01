using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractDetector : MonoBehaviour
{
    // 調べることが出来るもの(NPCや宝箱など)をまとめるためのリスト
    private List<GameObject> currentTargets = new List<GameObject>();
    
    private MainMove player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // MainMoveがついているObjectを探索、playerと命名
        player = UnityEngine.Object.FindFirstObjectByType<MainMove>();
    }
    public void OnInteract()
    {
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
            // 親ObjectのPlayerの位置座標を渡して、NPC側で計算してもらう
            npc.LookAtPlayer(transform.root.position);
            // NPCからjsonファイルを取得
            TextAsset json = npc.GetStoryJson();
            // jsonファイルが見つかったら
            if(json != null)
            {
                Debug.Log(npc.GetNPCName() + "会話を開始します");

                InkManager.Instance.SetShopNPC(npc.GetComponent<ShopNPC>());
                //InkManagerに会話を始めさせる
                InkManager.Instance.StartStory(json);

                // GameManagerで会話モードに変更
                GameManager.Instance.ChangeState(npc.GetInteractionState());
            }
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
