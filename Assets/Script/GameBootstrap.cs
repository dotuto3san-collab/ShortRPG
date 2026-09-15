using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string initialMapSceneName = "Town";

    IEnumerator Start()
    {
        string myOwnSceneName = gameObject.scene.name;

        for (int i = 0;i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if(s.name != myOwnSceneName)
            {
                if(SceneTransitionManager.Instance != null && string.IsNullOrEmpty(SceneTransitionManager.Instance.CurrentMapSceneName))
                {
                    SceneTransitionManager.Instance.RegisterInitialMapScene(s.name);
                    SceneTransitionManager.Instance.RewireMapReferences();
                }
                yield break;
            }
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(initialMapSceneName, LoadSceneMode.Additive);
        while (op != null && !op.isDone)
        {
            yield return null;
        }

        Scene mapScene = SceneManager.GetSceneByName(initialMapSceneName);
        if (mapScene.IsValid())
        {
            SceneManager.SetActiveScene(mapScene);
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.RegisterInitialMapScene(initialMapSceneName);
            SceneTransitionManager.Instance.RewireMapReferences();
        }
    }
}
