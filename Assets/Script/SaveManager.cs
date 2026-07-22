using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string path;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        path = Application.persistentDataPath + "/save.json";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if(GameManager.Instance.CurrentState == GameState.Exploring)
            {
                Save();
            }
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            Load();
        }
    }

    public void PublicSave()
    {
        Save();
    }

    public void OnClickSave()
    {
        if(GameManager.Instance != null &&
           !GameManager.Instance.CanSave())
        {
            Debug.Log($"åªç›ÇÃèÛë‘Ç≈ÇÕÉZÅ[ÉuÇ≈Ç´Ç‹ÇπÇÒ : {GameManager.Instance.CurrentState}");
            return;
        }

        Save();
    }

    public void Save()
    {
        SaveData data = new SaveData();

        data.money = GameManager.Instance.Money;
        data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if(InventoryManager.Instance != null)
        {
            data.items = InventoryManager.Instance.GetSaveData();
        }
        else
        {
            Debug.LogError("InventoryManager not found during Save");
        }

        MainMove player = GameManager.Instance.Player;
        if (player != null)
        {
            data.playerPosition = player.transform.position;
        }
        else
        {
            Debug.LogError("Player not found during Save");
        }

            string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("Saved: " + path);

        if(SaveFeedbackUI.Instance != null)
        {
            SaveFeedbackUI.Instance.Show();
        }
    }

    public void Load()
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if(data == null)
        {
            Debug.LogError("Failed to parse save data");
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(data.sceneName);

        StartCoroutine(LoadAfterScene(data));
    }

    private System.Collections.IEnumerator LoadAfterScene(SaveData data)
    {
        yield return null;

        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            InventoryManager.Instance != null);

        GameManager.Instance.SetMoney(data.money);
        InventoryManager.Instance.LoadFromSaveData(data.items);

        var player = FindFirstObjectByType<MainMove>();
        if(player != null)
        {
            player.transform.position = data.playerPosition;
        }

        GameManager.Instance.ChangeState(GameState.Exploring);
    }
}
