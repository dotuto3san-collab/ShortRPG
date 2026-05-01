using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
    public void OnPanelOpenComplete()
    {
        // InkManagerのInstanceがあるなら
        if(InkManager.Instance != null)
        {
            // 会話ウィンドウが開いたフラグを立てる関数を実行
            InkManager.Instance.OnPanelOpenComplete();
        }
    }
}
