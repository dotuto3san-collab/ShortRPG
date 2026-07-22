using UnityEngine;
using UnityEngine.Tilemaps;

public class EncounterAreaManager : MonoBehaviour
{
    public static EncounterAreaManager Instance;

    [SerializeField] private Tilemap encounterTilemap;

    void Awake()
    {
        Instance = this;
    }

    public string GetCurrentArea(Vector3 worldPos)
    {
        Vector3Int cell = encounterTilemap.WorldToCell(worldPos);

        var tile = encounterTilemap.GetTile<EncounterAreaTile>(cell);

        if (tile == null) return "";

        return tile.encounterGroupID;
    }
}
