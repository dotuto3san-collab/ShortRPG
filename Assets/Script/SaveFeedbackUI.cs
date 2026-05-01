using UnityEngine;
using TMPro;
using System.Collections;

public class SaveFeedbackUI : MonoBehaviour
{
    public static SaveFeedbackUI Instance;

    [SerializeField] private GameObject root;
    [SerializeField] private float displayTime = 2f;

    private Coroutine currentCoroutine;

    void Awake()
    {
        Instance = this;

        if(root != null)
        {
            root.SetActive(false);
        }
    }

    public void Show()
    {
        if (root == null) return;

        if(currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        root.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        root.SetActive(false);
    }
}
