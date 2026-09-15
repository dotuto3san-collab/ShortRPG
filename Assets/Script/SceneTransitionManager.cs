using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("ââèoê›íË")]
    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private float delayBeforeTime = 0.3f;
    [SerializeField] private float delayAfterTime = 0.3f;

    [Header("ÉJÉÅÉâê›íË")]
    [SerializeField] private Unity.Cinemachine.CinemachineCamera followCamera;

    public bool IsTransitioning { get; private set; }

    public string CurrentMapSceneName { get; private set; }

    void Awake()
    {
        if(Instance != null &&  Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterInitialMapScene(string sceneName)
    {
        CurrentMapSceneName = sceneName;
    }

    public void RequestTransition(string targetSceneName, Vector2 targetPosition)
    {
        if (IsTransitioning) return;

        StartCoroutine(TransitionRoutine(targetSceneName, targetPosition));
    }

    private IEnumerator UnloadAllMapScenes()
    {
        string myOwnSceneName = gameObject.scene.name;

        List<string> scenesToUnload = new List<string>();
        for(int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if(s.name != myOwnSceneName)
            {
                scenesToUnload.Add(s.name);
            }
        }

        foreach(string sceneName in scenesToUnload)
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(sceneName);
            if(sceneToUnload.IsValid() && sceneToUnload.isLoaded)
            {
                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToUnload);
                while(unloadOp != null && !unloadOp.isDone)
                {
                    yield return null;
                }
            }
        }

        CurrentMapSceneName = null;
    }

    private IEnumerator TransitionRoutine(string targetSceneName, Vector2 targetPosition)
    {
        IsTransitioning = true;
        GameManager.Instance.SetSceneTransitioning(true);

        MainMove oldPlayer = GameManager.Instance.Player;
        if (oldPlayer != null) oldPlayer.isInputLocked = true;

        if (fadeAnimator != null)
        {
            fadeAnimator.gameObject.SetActive(true);
            fadeAnimator.Play("FadeOut");
        }
        yield return new WaitForSeconds(delayBeforeTime);

        yield return UnloadAllMapScenes();

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        while (loadOp != null && !loadOp.isDone)
        {
            yield return null;
        }

        Scene newScene = SceneManager.GetSceneByName(targetSceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }

        CurrentMapSceneName = targetSceneName;

        MainMove newPlayer = Object.FindFirstObjectByType<MainMove>();

        if (newPlayer != null)
        {
            newPlayer.isInputLocked = true;

            Vector3 previousPos = newPlayer.transform.position;
            newPlayer.transform.position = targetPosition;

            RewireMapReferences();
        }
        else
        {
            Debug.LogError($"SceneTransitionManager: {targetSceneName}Ç…PlayerÇ™å©Ç¬Ç©ÇËÇ‹ÇπÇÒ");
        }

        if (fadeAnimator != null)
        {
            fadeAnimator.gameObject.SetActive(true);
            fadeAnimator.Play("FadeIn");
        }
        yield return new WaitForSeconds(delayAfterTime);

        if (newPlayer != null)
        {
            newPlayer.isInputLocked = false;
        }

        if (fadeAnimator != null)
        {
            fadeAnimator.gameObject.SetActive(false);
        }

        IsTransitioning = false;
        GameManager.Instance.SetSceneTransitioning(false);
    }

    public void LoadForSaveData(string targetSceneName, Vector3 targetPosition, System.Action onComplete)
    {
        StartCoroutine(LoadForSaveDataRoutine(targetSceneName, targetPosition, onComplete));
    }

    private IEnumerator LoadForSaveDataRoutine(string targetSceneName, Vector3 targetPosition,System.Action onComplete)
    {
        IsTransitioning = true;
        GameManager.Instance.SetSceneTransitioning(true);

        yield return UnloadAllMapScenes();

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        while(loadOp != null && !loadOp.isDone)
        {
            yield return null;
        }

        Scene newScene = SceneManager.GetSceneByName(targetSceneName);
        if (newScene.IsValid())
        {
            SceneManager.SetActiveScene(newScene);
        }

        CurrentMapSceneName = targetSceneName;

        MainMove newPlayer = Object.FindFirstObjectByType<MainMove>();
        if(newPlayer != null)
        {
            newPlayer.isInputLocked = true;
            newPlayer.transform.position = targetPosition;

            RewireMapReferences();
        }

        IsTransitioning = false;
        GameManager.Instance.SetSceneTransitioning(false);

        onComplete?.Invoke();
    }

    public void RewireMapReferences()
    {
        MapEncounterTilemap marker = Object.FindFirstObjectByType<MapEncounterTilemap>();

        if(EncounterAreaManager.Instance != null)
        {
            EncounterAreaManager.Instance.SetEncounterTilemap(marker != null ? marker.Tilemap : null);
        }

        MainMove player = Object.FindFirstObjectByType<MainMove>();

        if(player != null && followCamera != null)
        {
            followCamera.Follow = player.transform;

            Vector3 warpDelta = player.transform.position - followCamera.transform.position;
            followCamera.OnTargetObjectWarped(player.transform, warpDelta);
        }
    }

    public void RequestEventFade(System.Action midAction, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
    {
        StartCoroutine(EventFadeRoutine(midAction, fadeOutDuration, fadeInDuration));
    }

    public IEnumerator EventFadeRoutine(System.Action midAction, float fadeOutDuration = 0.5f, float fadeInDuration = 0.5f)
    {
        if(fadeAnimator != null)
        {
            fadeAnimator.gameObject.SetActive(true);
            fadeAnimator.Play("FadeOut");
        }
        yield return new WaitForSeconds(fadeOutDuration);

        midAction?.Invoke();

        if(fadeAnimator != null)
        {
            fadeAnimator.Play("FadeIn");
        }
        yield return new WaitForSeconds(fadeInDuration);

        if(fadeAnimator != null)
        {
            fadeAnimator.gameObject.SetActive(false);
        }
    }

    public void FocusCameraOn(Transform target)
    {
        if (followCamera == null || target == null) return;

        followCamera.Follow = target;

        Vector3 warpDelta = target.position - followCamera.transform.position;
        followCamera.OnTargetObjectWarped(target, warpDelta);
    }

    public void ReturnCameraToPlayer()
    {
        MainMove player = GameManager.Instance != null ? GameManager.Instance.Player : null;

        if(player == null)
        {
            player = Object.FindFirstObjectByType<MainMove>();
        }

        if (player == null) return;

        FocusCameraOn(player.transform);
    }
}
