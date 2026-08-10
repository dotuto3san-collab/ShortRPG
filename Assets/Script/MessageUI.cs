using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
    public static MessageUI Instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI messageText;

    [SerializeField] private Animator nextIconAnimator;

    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI effectText;

    public bool IsShowing { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public IEnumerator ShowMessage(string message)
    {
        itemIcon.gameObject.SetActive(false);
        effectText.gameObject.SetActive(false);

        IsShowing = true;

        panel.SetActive(true);
        messageText.text = message;

        if(nextIconAnimator != null &&
           nextIconAnimator.gameObject.activeInHierarchy)
        {
            nextIconAnimator.Play("NextIcon_Hidden");
        }

        while (Input.GetKey(KeyCode.Z) ||
             Input.GetKey(KeyCode.Return))
        {
            yield return null;
        }

        bool waitingForClose = true;

        if(nextIconAnimator != null &&
           nextIconAnimator.gameObject.activeInHierarchy)
        {
            nextIconAnimator.Play("NextIcon_Blink");
        }

        while (waitingForClose)
        {
            if(Input.GetKeyDown(KeyCode.Z) ||
               Input.GetKeyDown(KeyCode.Return))
            {
                waitingForClose = false;
            }

            yield return null;
        }

        if(nextIconAnimator != null && nextIconAnimator.gameObject.activeInHierarchy)
        {
            nextIconAnimator.Play("NextIcon_Hidden");
        }

        panel.SetActive(false);

        IsShowing = false;
    }

    public IEnumerator ShowMessage(string message, Sprite icon, string effect)
    {
        IsShowing = true;

        panel.SetActive(true);

        messageText.text = message;

        itemIcon.gameObject.SetActive(icon != null);
        itemIcon.sprite = icon;

        effectText.gameObject.SetActive(!string.IsNullOrEmpty(effect));
        effectText.text = effect;

        if (nextIconAnimator != null)
            nextIconAnimator.Play("NextIcon_Hidden");

        while (Input.GetKey(KeyCode.Z) ||
             Input.GetKey(KeyCode.Return))
        {
            yield return null;
        }

        bool waitingForClose = true;

        if (nextIconAnimator != null)
        {
            nextIconAnimator.Play("NextIcon_Blink");
        }

        while (waitingForClose)
        {
            if (Input.GetKeyDown(KeyCode.Z) ||
               Input.GetKeyDown(KeyCode.Return))
            {
                waitingForClose = false;
            }

            yield return null;
        }

        if (nextIconAnimator != null)
        {
            nextIconAnimator.Play("NextIcon_Hidden");
        }

        panel.SetActive(false);

        IsShowing = false;
    }
}
