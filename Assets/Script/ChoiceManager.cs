using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Ink.Runtime;
using System;
using System.Collections.Generic;

public class ChoiceManager : MonoBehaviour
{
    [Header("UI設定")]
    // ボタンの元となるObjectを参照
    [SerializeField] private GameObject choiceButtonPrefab; 
    // ボタンを並べる親要素Objectを参照
    [SerializeField] private Transform choiceContainer; 

    // ボタンが押されたことを外部クラスに伝える
    public event Action<int> OnChoiceSelected;
    // ボタンを表示する命令
    public void CreateChoices(List<Choice> choices)
    {
        // 前のボタンが残っていたら消す関数を実行
        ClearChoices();
        // 最初のボタンを覚える変数
        GameObject firstButton = null;

        // ボタンを生成する
        foreach (Choice choice in choices)
        {
            // ボタンを生成してContainerの子にする
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer);
            // 最初の一個を記憶
            if (firstButton == null) firstButton = buttonObj;

            // buttonのテキストを取得
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            // ボタンに書いてあるNewTextをInkの選択肢文に書き換える
            if (buttonText != null) buttonText.text = choice.text;

            // UnityのButtonコンポーネントに、クリックされた時の動きを登録する
            int index = choice.index;
            // 自動的にリストを作成、各選択肢に番号を割り振る
            buttonObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                // ボタンが押されたことを通知
                OnChoiceSelected?.Invoke(index);
            });
        }
        // とりあえず一番目のボタンを仮選択状態(Zキーを押せば選択される状態)にする
        if(firstButton != null)
        {
            // 最初に生成されたボタンを開始地点とする
            EventSystem.current.SetSelectedGameObject(firstButton);
            // 選択されたボタンのButton機能だけ参照
            Button btn = firstButton.GetComponent<Button>();
            // もしbtnがnullでなければ
            if(btn != null)
            {
                // 選ばれたボタンを実際に選択状態にする
                btn.Select();
                // Select状態時の見た目の変化を適応
                btn.OnSelect(null);
            }
        }
    }

    // 分岐選択が終わったら、ボタンを削除する関数
    public void ClearChoices()
    {
        // 生成した個数分ボタン繰り返す
        foreach(Transform child in choiceContainer)
        {
            // 生成した個数分ボタンを消す
            Destroy(child.gameObject);
        }
    }
}
