using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBootstrap : MonoBehaviour
{
    [SerializeField] private string persistentSceneName = "PersistentScene";

    [SerializeField] private string thisMapSceneName;

    void Awake()
    {
        Scene persistentScene = SceneManager.GetSceneByName(persistentSceneName);

        if (!persistentScene.isLoaded)
        {
            SceneManager.LoadScene(persistentSceneName, LoadSceneMode.Additive);
        }

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.RegisterInitialMapScene(thisMapSceneName);
            SceneTransitionManager.Instance.RewireMapReferences();
        }
    }
}
