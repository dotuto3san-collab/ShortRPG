using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneMove : Editor
{
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

    // Tarts_OutdoorをUnityMenuのSceneから開けるようにする
    [MenuItem("Scene/Town/Tarts_Outdoor",false,3)]
    public static void LoadTartsOut()
    {
        // もしEditor上で変更を加えた場合に保存するかしないかを問う
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 指定されたPasのSceneへ移動
            EditorSceneManager.OpenScene("Assets/Scenes/Tarts/Tarts_Outdoor.unity");
        }
    }

    // Tarts_IndoorをUnityMenuのSceneから開けるようにする
    [MenuItem("Scene/Town/Tarts_Indoor",false,4)]
    public static void LoadTartsIn()
    {
        // もしEditor上で変更を加えた場合に保存するかしないかを問う
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // 指定されたPasのSceneへ移動
            EditorSceneManager.OpenScene("Assets/Scenes/Tarts/Tarts_Indoor.unity");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
