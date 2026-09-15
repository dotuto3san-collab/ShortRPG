using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class MapEncounterTilemap : MonoBehaviour
{
    private Tilemap tilemap;

    public Tilemap Tilemap
    {
        get
        {
            if(tilemap == null) tilemap = GetComponent<Tilemap>();
            return tilemap;
        }
    }
}
