using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
    public struct WorldTile {
        public bool present_upper;
        public bool present_lower;
        public TileBehavior upper_tile;
        public TileBehavior lower_tile;
    }

    public WorldTile [,] map;
    public static WorldMap instance;

    void Awake()
    {
        instance = this;
        map = new WorldTile[100, 100];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PublishTile(Vector3Int coords, GameObject obj) {
        TileBehavior tile_behavior = obj.GetComponent<TileBehavior>();
        if (tile_behavior.IsUpper) {
            map[coords.x + 50, coords.y + 50].present_upper = true;
            map[coords.x + 50, coords.y + 50].upper_tile = tile_behavior;
        } else {
            map[coords.x + 50, coords.y + 50].present_lower = true;
            map[coords.x + 50, coords.y + 50].lower_tile = tile_behavior;
        }
    }

    public void UnPublishTile(Vector3Int coords, GameObject obj) {
        WorldTile tile = map[coords.x + 50, coords.y + 50];
        if (tile.upper_tile.gameObject == obj) {
            tile.present_upper = false;
            tile.upper_tile = null;
        } else if (tile.lower_tile.gameObject == obj) {
            tile.present_lower = false;
            tile.lower_tile = null;
        }
    }

    public TileBehavior GetTopTile(Vector3Int coords) {
        WorldTile tile = map[coords.x + 50, coords.y + 50];
        if (tile.present_upper) {
            return tile.upper_tile;
        } else if (tile.present_lower){
            return tile.lower_tile;
        }
        return null;
    }
}