using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileBehavior : MonoBehaviour
{
    public static Vector3Int [] NEIGHBOR_COORDS = {
        Vector3Int.left,
        Vector3Int.left + Vector3Int.up,
        Vector3Int.up,
        Vector3Int.up + Vector3Int.right,
        Vector3Int.right,
        Vector3Int.right + Vector3Int.down,
        Vector3Int.down,
        Vector3Int.down + Vector3Int.left,
    };

    public enum VillagerTargetType {
        DONT_CARE = 0,
        BUILDING,
        WATER
    }

    public enum PathAble
    {
        BLOCKS_MOVEMENT = 0,
        ALLOWS_MOVEMENT = 1,
    }

    public PathAble CanPath;
    public VillagerTargetType VillagerTarget = VillagerTargetType.DONT_CARE;
    public float MovementModifier = 0;
    [Header("Do Not Edit")]
    public IsometricRuleTile ThisTile;
    public Vector3Int IsoCoordinates;
    public Vector3 WorldCoordinates;
    public bool IsUpper;
    public FireBehaviour Fire;
    public Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
        if(tilemap == null){tilemap = GetComponentInParent<Tilemap>();}

        Fire = GetComponent<FireBehaviour>();
        WorldCoordinates = transform.position;
        IsoCoordinates = tilemap.WorldToCell(WorldCoordinates);
        ThisTile = tilemap.GetTile<IsometricRuleTile>(IsoCoordinates); //Unused


        if (transform.parent == WorldMap.instance.transform)
        {
            //We are the one true gameobject. We get to live on
            WorldMap.instance.PublishTile(IsoCoordinates, gameObject);
        }
        else
        {
            if (WorldMap.instance.tileAlreadyExists(IsoCoordinates, gameObject))
            {
                //Delete self. You're ugly. you're Disgusting. I'm going to Kill you. Give me $200
                Destroy(gameObject);
            }
            else
            {
                //Move over to the correct parent (worldmap), then delete self
                GameObject theChosenOne = Instantiate(gameObject);
                theChosenOne.transform.parent = WorldMap.instance.transform;
                theChosenOne.transform.position = transform.position;
                theChosenOne.transform.rotation = transform.rotation;
                TilemapRenderer renderer = GetComponentInParent<TilemapRenderer>();

                //Note which tilemap we used to be a part of
                TileBehavior chosenOneTileBehav = theChosenOne.GetComponent<TileBehavior>();

                chosenOneTileBehav.tilemap = tilemap;

                if (renderer.sortingLayerName == "Default"){IsUpper = false;}
                else if (renderer.sortingLayerName == "Buildings"){IsUpper = true;}
                else{ Debug.LogWarning("TilemapRenderer in parent does not have a valid sorting layer");}
                chosenOneTileBehav.IsUpper = this.IsUpper;
                Destroy(gameObject);
            }
        }

 
            
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

    public void DeleteTile()
    {
        WorldMap.instance.UnPublishTile(IsoCoordinates, gameObject);
        tilemap.SetTile(IsoCoordinates, null);
        Destroy(gameObject);
    }

    public bool hasBurningNeighbor()
    {
        foreach(TileBehavior neighbor in GetNeighbors())
        {
            if(neighbor.Fire.state == FireBehaviour.burnState.burning) return true;
        }
        return false;
    }
}