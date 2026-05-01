using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.UI;
using System.Collections;

public class BasicInkIteration : MonoBehaviour
{
    [SerializeField] private TextAsset inkJson;
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private float typingSpeed = 0.05f; // 表示速度

    private Story story;
    private Coroutine typingCoroutine;
    private bool isTyping = false; // 現在文字送り中か

    void Awake()
    {
        if (inkJson != null)
        {
            story = new Story(inkJson.text);
            RefreshView();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // 文字送り中にスペースを押したら、一気に全表示
                CompleteLine();
            }
            else if (story.canContinue)
            {
                RefreshView();
            }
        }
    }

    void RefreshView()
    {
        // 古いボタンを削除
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }

        if (story.canContinue)
        {
            string nextLine = story.Continue();
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(DisplayLine(nextLine));
        }
    }

    IEnumerator DisplayLine(string line)
    {
        textDisplay.text = "";
        isTyping = true;

        foreach (char letter in line.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        CheckForChoices();
    }

    void CompleteLine()
    {
        StopCoroutine(typingCoroutine);
        // story.currentText には最後に出した一文が入っています
        textDisplay.text = story.currentText;
        isTyping = false;
        CheckForChoices();
    }

    void CheckForChoices()
    {
        if (!story.canContinue && story.currentChoices.Count > 0)
        {
            foreach (Choice choice in story.currentChoices)
            {
                GameObject button = Instantiate(choiceButtonPrefab, choiceContainer);
                button.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
                int index = choice.index;
                button.GetComponent<Button>().onClick.AddListener(() => OnClickChoice(index));
            }
        }
    }

    void OnClickChoice(int index)
    {
        story.ChooseChoiceIndex(index);
        RefreshView();
    }
}
