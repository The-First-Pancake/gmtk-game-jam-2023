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

    [Header("Do Not Edit")]
    public PathAble CanPath;
    public IsometricRuleTile ThisTile;
    public Vector3Int IsoCoordinates;
    public Vector3 WorldCoordinates;
    public bool IsUpper;
    // Start is called before the first frame update
    void Start()
    {
        Transform transform = GetComponent<Transform>();
        WorldCoordinates = transform.position;
        Tilemap tilemap = GetComponentInParent<Tilemap>();
        IsoCoordinates = tilemap.WorldToCell(WorldCoordinates);
        ThisTile = tilemap.GetTile<IsometricRuleTile>(IsoCoordinates);
        TilemapRenderer renderer = GetComponentInParent<TilemapRenderer>();
        if (renderer.sortingLayerName == "Default") {
            IsUpper = false;
        } else if (renderer.sortingLayerName == "Buildings") {
            IsUpper = true;
        } else {
            Debug.LogWarning("TilemapRenderer in parent does not have a valid sorting layer");
        }
        WorldMap.instance.PublishTile(IsoCoordinates, gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<TileBehavior> GetNeighbors() {
        List<TileBehavior> neighbors = new List<TileBehavior>();
        foreach (Vector3Int coord in NEIGHBOR_COORDS) {
            TileBehavior tile = WorldMap.instance.GetTopTile(coord + IsoCoordinates);
            if (tile) {
                neighbors.Add(tile);
            }
        }
        return neighbors;
    }
}