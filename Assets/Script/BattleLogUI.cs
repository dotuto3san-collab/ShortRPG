using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BattleLogUI : MonoBehaviour
{
    public static BattleLogUI Instance;

    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] float displayTime = 2;

    [SerializeField] private Animator nextIconAnimator;

    private Queue<LogEntry> messageQueue = new Queue<LogEntry>();
    private Coroutine currentCoroutine;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public IEnumerator ShowLogAndWait(string message, bool waitForInput = false)
    {
        bool done = false;
        messageQueue.Enqueue(new LogEntry(message, waitForInput, () => done = true));

        if (currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(ProcessLogs());
        }

        yield return new WaitUntil(() => done);
    }

    public void ShowImmediate(string message)
    {
        if(currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        messageQueue.Clear();

        logText.text = message;
    }

    private IEnumerator ProcessLogs()
    {
        while (messageQueue.Count > 0)
        {
            var entry = messageQueue.Dequeue();
            logText.text = entry.message;

            if (entry.waitForInput)
            {
                yield return null;

                if(nextIconAnimator != null)
                {
                    nextIconAnimator.Play("NextIcon_Blink");
                }

                yield return new WaitUntil(() =>
                    Input.GetKeyDown(KeyCode.Z) ||
                    Input.GetKeyDown(KeyCode.Return)
                );

                if(nextIconAnimator != null)
                {
                    nextIconAnimator.Play("NextIcon_Hidden");
                }
            }
            else
            {
                if(nextIconAnimator != null)
                {
                    nextIconAnimator.Play("NextIcon_Hidden");
                }

                yield return new WaitForSeconds(displayTime);
            }

            entry.onComplete?.Invoke();
        }

        if(nextIconAnimator != null)
        {
            nextIconAnimator.Play("NextIcon_Hidden");
        }

        currentCoroutine = null;
    }

    private class LogEntry
    {
        public string message;
        public bool waitForInput;
        public System.Action onComplete;

        public LogEntry(string message, bool waitForInput, System.Action onComplete = null)
        {
            this.message = message;
            this.waitForInput = waitForInput;
            this.onComplete = onComplete;
        }
    }
}
