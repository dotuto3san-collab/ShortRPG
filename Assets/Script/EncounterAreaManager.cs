using UnityEngine;
using UnityEngine.Tilemaps;

public class EncounterAreaManager : MonoBehaviour
{
    public static EncounterAreaManager Instance;

    private Tilemap encounterTilemap;

    void Awake()
    {
        Instance = this;
    }

    public void SetEncounterTilemap(Tilemap tilemap)
    {
        encounterTilemap = tilemap;
    }

    public string GetCurrentArea(Vector3 worldPos)
    {
        if (encounterTilemap == null) return "";

        Vector3Int cell = encounterTilemap.WorldToCell(worldPos);
        var tile = encounterTilemap.GetTile<EncounterAreaTile>(cell);
        if (tile == null) return "";
        return tile.encounterGroupID;
    }
}
