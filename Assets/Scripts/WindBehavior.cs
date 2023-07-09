using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindBehavior : MonoBehaviour
{
    private Vector3Int windDir;

    // Start is called before the first frame update
    void Start()
    {
        windDir = TileBehavior.NEIGHBOR_COORDS[UnityEngine.Random.Range(0, TileBehavior.NEIGHBOR_COORDS.Length - 1)];
        InvokeRepeating("WindChange", .5f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void WindChange()
    {
        if(UnityEngine.Random.Range(0,100) > 95){
            int index = Array.IndexOf(TileBehavior.NEIGHBOR_COORDS, windDir);
            int noise = UnityEngine.Random.Range(-1, 2);
            int newIndex = mod(index + noise, TileBehavior.NEIGHBOR_COORDS.Length); //wraps index at length of array
            windDir = TileBehavior.NEIGHBOR_COORDS[newIndex];
        }
        
    }
    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    public Vector3 GetWorldWindDir()
    {
        return WorldMap.instance.grid.CellToWorld(windDir);
    }
    public Vector3Int GetIsoWindDir()
    {
        return windDir;
    }
}
