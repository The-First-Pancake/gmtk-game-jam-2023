using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileBehavior : MonoBehaviour
{
    enum PathAble
    {
        BLOCKS_MOVEMENT = 0,
        ALLOWS_MOVEMENT = 1,
    }
    PathAble CanPath;
    IsometricRuleTile ThisTile;
    Vector3Int IsoCoordinates;
    Vector3 WorldCoordinates;

    void Awake()
    {
        Transform transform = GetComponent<Transform>();
        WorldCoordinates = transform.position;
        Grid worldGrid = GetComponentInParent<Grid>();
        IsoCoordinates = worldGrid.WorldToCell(WorldCoordinates);
        Tilemap tilemap = GetComponentInParent<Tilemap>();
        ThisTile = tilemap.GetTile<IsometricRuleTile>(IsoCoordinates);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
