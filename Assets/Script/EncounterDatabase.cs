using UnityEngine;


public class EncounterDatabase : MonoBehaviour
{
    public static EncounterDatabase Instance;

    [SerializeField] private EncounterAreaData[] areas;

    void Awake()
    {
        Instance = this;
    }

    public EncounterAreaData GetArea(string areaID)
    {
        foreach (var area in areas)
        {
            if(area.areaID == areaID)
            {
                return area;
            }
        }

        return null;
    }
}
