using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneMove : Editor
{
    [MenuItem("Scene/PersistentScene", false, 1)]
    public static void LoadPersistent()
    {
        // もしEditor上で変更を加えた場合に保存するかしないかを問う
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 指定されたPasのSceneへ移動
            EditorSceneManager.OpenScene("Assets/Scenes/PersistentScene.unity");
        }
    }

    [MenuItem("Scene/PortalTestScene", false, 2)]
    public static void LoadPortalTest()
    {
        // もしEditor上で変更を加えた場合に保存するかしないかを問う
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 指定されたPasのSceneへ移動
            EditorSceneManager.OpenScene("Assets/Scenes/PortalTestScene.unity");
        }
    }

    // Tarts_OutdoorをUnityMenuのSceneから開けるようにする
    [MenuItem("Scene/Tarts_Outdoor", false, 3)]
    public static void LoadTartsOut()
    {
        // もしEditor上で変更を加えた場合に保存するかしないかを問う
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 指定されたPasのSceneへ移動
            EditorSceneManager.OpenScene("Assets/Scenes/Tarts/Tarts_Outdoor.unity");
        }
    }

    // Tutorial_OutdoorをUnityMenuのSceneから開けるようにする
    [MenuItem("Scene/Town/Tutorial_Outdoor",false,1)]
    public static void LoadTutorialOut()
    {
        // もしEditor上で変更を加えた場合に保存するかしないかを問う
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 指定されたPasのSceneへ移動
            EditorSceneManager.OpenScene("Assets/Scenes/Tutorial/Tutorial_Outdoor.unity");
        }
    }
    
    // Tutorial_IndoorをUnityMenuのSceneから開けるようにする
    [MenuItem("Scene/Town/Tutorial_Indoor",false,2)]
    public static void LoadTutorialIn()
    {
        // もしEditor上で変更を加えた場合に保存するかしないかを問う
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 指定されたPasのSceneへ移動
            EditorSceneManager.OpenScene("Assets/Scenes/Tutorial/Tutorial_Indoor.unity");
        }
    }

    // Tarts_IndoorをUnityMenuのSceneから開けるようにする
    [MenuItem("Scene/Town/Tarts_Indoor",false,3)]
    public static void LoadTartsIn()
    {
        // もしEditor上で変更を加えた場合に保存するかしないかを問う
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 指定されたPasのSceneへ移動
            EditorSceneManager.OpenScene("Assets/Scenes/Tarts/Tarts_Indoor.unity");
        }
    }
}
