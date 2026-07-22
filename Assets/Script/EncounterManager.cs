using System.Collections.Generic;
using UnityEngine;

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance;

    [Header("êÌì¨óp")]
    [SerializeField] private BattleUnit playerPrefab;
    [SerializeField] private BattleUnit enemyPrefab;

    [Header("ï‡êî")]
    [SerializeField] private int minSteps = 5;
    [SerializeField] private int maxSteps = 12;

    private int currentSteps;
    private int nextEncounterStep;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetEncounterCount();
    }

    void ResetEncounterCount()
    {
        currentSteps = 0;
        nextEncounterStep = Random.Range(minSteps, maxSteps + 1);
    }

    public void OnPlayerStep(Vector3 playerPosition)
    {
        string areaID =
            EncounterAreaManager.Instance.GetCurrentArea(playerPosition);

        if (string.IsNullOrEmpty(areaID))
        {
            ResetEncounterCount();
            return;
        }

        currentSteps++;

        if(currentSteps < nextEncounterStep) return;

        StartEncounter(areaID);

        ResetEncounterCount();
    }

    void StartEncounter(string areaID)
    {
        EncounterAreaData area =
            EncounterDatabase.Instance.GetArea(areaID);

        if (area == null) return;

        if (area.encounterGroups.Length == 0) return;

        int rand = Random.Range(0,area.encounterGroups.Length);

        EncounterGroupData group = area.encounterGroups[rand];

        BattleStart(group);
    }

    void BattleStart(EncounterGroupData group)
    {
        var player = Instantiate(playerPrefab);
        player.Init();

        List<BattleUnit> enemies = new List<BattleUnit>();

        foreach(BattleData data in group.enemies)
        {
            BattleUnit enemy = Instantiate(enemyPrefab);

            enemy.data = data;
            enemy.Init();

            enemies.Add(enemy);
        }

        BattleManager.Instance.StartBattle(player,enemies);
    }
}
