using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldMap : MonoBehaviour
{
    public struct WorldTile {
        public bool present_upper;
        public bool present_lower;
        public TileBehavior upper_tile;
        public TileBehavior lower_tile;
    }

    public WorldTile [,] map;
    public Grid grid;
    public static WorldMap instance;


    void Awake()
    {
        instance = this;
        map = new WorldTile[100, 100];
        grid = GetComponentInParent<Grid>();
    }

    public void PublishTile(Vector3Int coords, GameObject obj) {
        if (tileOutOfBound(coords)) { return; };
        TileBehavior tile_behavior = obj.GetComponent<TileBehavior>();
        if (tile_behavior.IsUpper) {
            map[coords.x + 50, coords.y + 50].present_upper = true;
            map[coords.x + 50, coords.y + 50].upper_tile = tile_behavior;
        } else {
            map[coords.x + 50, coords.y + 50].present_lower = true;
            map[coords.x + 50, coords.y + 50].lower_tile = tile_behavior;
        }
    }

    public bool tileAlreadyExists(Vector3Int coords, GameObject obj)
    {
        if (tileOutOfBound(coords)) { return false; };
        TileBehavior tile_behavior = obj.GetComponent<TileBehavior>();
        if (tile_behavior.IsUpper)
        {
            if(map[coords.x + 50, coords.y + 50].present_upper == true)
            {
                return true;
            }
        }
        else
        {
            if (map[coords.x + 50, coords.y + 50].present_lower == true)
            {
                return true;
            }
        }
        return false;
    }

    public void UnPublishTile(Vector3Int coords, GameObject obj) {
        if (tileOutOfBound(coords)){return; };
        if (map[coords.x + 50, coords.y + 50].upper_tile.gameObject == obj) {
            Debug.Log("Unpublishing Upper");
            map[coords.x + 50, coords.y + 50].present_upper = false;
            map[coords.x + 50, coords.y + 50].upper_tile = null;
        } else if (map[coords.x + 50, coords.y + 50].lower_tile.gameObject == obj) {
            Debug.Log("Unpublishing Lower");
            map[coords.x + 50, coords.y + 50].present_lower = false;
            map[coords.x + 50, coords.y + 50].lower_tile = null;
        }
    }

    public bool tileOutOfBound(Vector3Int coords){
        bool output = (coords.x >= 50 || coords.x < -50 || coords.y >= 50 || coords.y < -50);
        if (output) {
            Debug.Log($"Attempted to apply worldmap change to out of bounds tile at {coords.x}, {coords.y}");
        }
        return (coords.x >= 50 || coords.x < -50 || coords.y >= 50 || coords.y < -50);
    }

    public TileBehavior GetTopTileFromWorldPoint(Vector3 coords) {
        Vector3Int iso_point = grid.WorldToCell(coords);
        return GetTopTile(iso_point);
    }

    public List<TileBehavior> GetAllTilesInCell(Vector3Int coords)
    {
        if (tileOutOfBound(coords)) { return null; };
        WorldTile tile = map[coords.x + 50, coords.y + 50];
        List<TileBehavior> tileBehaviors = new List<TileBehavior>();
        if (tile.present_upper)
        {
            tileBehaviors.Add( tile.upper_tile);
        }
        else if (tile.present_lower)
        {
            tileBehaviors.Add(tile.lower_tile);
        }
        return tileBehaviors;
    }

    public TileBehavior GetTopTile(Vector3Int coords) {
        if (tileOutOfBound(coords)) { return null;};
        WorldTile tile = map[coords.x + 50, coords.y + 50];
        if (tile.present_upper) {
            return tile.upper_tile;
        } else if (tile.present_lower){
            return tile.lower_tile;
        }
        return null;
    }

    public List<TileBehavior> GetAllTilesOfTargetType(TileBehavior.VillagerTargetType type) {
        List<TileBehavior> targets = new List<TileBehavior>();
        foreach (WorldTile tile in map) {
            if (tile.present_upper) {
                if (tile.upper_tile.VillagerTarget == type) {
                    targets.Add(tile.upper_tile);
                }
            } else if (tile.present_lower) {
                if (tile.lower_tile.VillagerTarget == type) {
                    targets.Add(tile.lower_tile);
                }
            }
        }
        return targets;
    }

    public List<TileBehavior> GetAllBurningTiles() {
        List<TileBehavior> targets = new List<TileBehavior>();
        foreach (WorldTile tile in map) {
            if (tile.present_upper) {
                if (tile.upper_tile.Fire.state == FireBehaviour.burnState.burning) {
                    targets.Add(tile.upper_tile);
                }
            } else if (tile.present_lower) {
                if (tile.lower_tile.Fire.state == FireBehaviour.burnState.burning) {
                    targets.Add(tile.lower_tile);
                }
            }
        }
        return targets;
    }
}
