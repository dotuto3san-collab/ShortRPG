using UnityEngine;
using System.Collections.Generic;

public class EncounterTester : MonoBehaviour
{
    public BattleUnit playerPrefab;
    public List<BattleData> enemyDataList;
    public BattleUnit enemyPrefab;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            var p = Instantiate(playerPrefab);
            Debug.Log("Init‘O isPlayer: " + p.isPlayer);
            p.Init();

            List<BattleUnit> enemies = new List<BattleUnit>();

            int enemyCount = Random.Range(1, 5);

            for(int i = 0; i < enemyCount; i++)
            {
                var e = Instantiate(enemyPrefab);

                int rand = Random.Range(0,enemyDataList.Count);
                e.data = enemyDataList[rand];

                e.Init();
                enemies.Add(e);
            }

            BattleManager.Instance.StartBattle(p, enemies);
        }
    }
}
