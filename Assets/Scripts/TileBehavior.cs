using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileBehavior : MonoBehaviour
{
    static Vector3Int [] NEIGHBOR_COORDS = {
        Vector3Int.left,
        Vector3Int.left + Vector3Int.up,
        Vector3Int.up,
        Vector3Int.up + Vector3Int.right,
        Vector3Int.right,
        Vector3Int.right + Vector3Int.down,
        Vector3Int.down,
        Vector3Int.down + Vector3Int.left,
    };

    public enum PathAble
    {
        BLOCKS_MOVEMENT = 0,
        ALLOWS_MOVEMENT = 1,
    }
    public PathAble CanPath;
    [HideInInspector] public IsometricRuleTile ThisTile;
    [HideInInspector] public Vector3Int IsoCoordinates;
    [HideInInspector] public Vector3 WorldCoordinates;
    // Start is called before the first frame update
    void Start()
    {
        Transform transform = GetComponent<Transform>();
        WorldCoordinates = transform.position;
        Tilemap tilemap = GetComponentInParent<Tilemap>();
        IsoCoordinates = tilemap.WorldToCell(WorldCoordinates);
        ThisTile = tilemap.GetTile<IsometricRuleTile>(IsoCoordinates);
        WorldMap.instance.PublishTile(IsoCoordinates, gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsUpper() {
        return true; // TODO What is upper
    }

    public List<TileBehavior> GetNeighbors() {
        List<TileBehavior> neighbors = new List<TileBehavior>();
        foreach (Vector3Int coord in NEIGHBOR_COORDS) {
            TileBehavior tile = WorldMap.instance.GetTopTile(coord);
            if (tile) {
                neighbors.Add(tile);
            }
        }
        return neighbors;
    }
}