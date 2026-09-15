using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("移動設定")]
    // Inspectorにて移動先のScene名を入力
    public string targetSceneName;
    // Inspectorにて移動先のSceneのワープ地点座標を入力
    public Vector2 targetPosition;

    // ワープの準備を開始
    private void OnTriggerEnter2D(Collider2D other)
    {
        // もしプレイヤーが触れて、かつ移動を開始していない場合に移動準備コルーチン開始
        if (other.GetComponent<MainMove>() == null) return;

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("SceneTransitionManager.Instanceが存在しません");
            return;
        }

        if (SceneTransitionManager.Instance.IsTransitioning) return;

        SceneTransitionManager.Instance.RequestTransition(targetSceneName, targetPosition);
    }
}
