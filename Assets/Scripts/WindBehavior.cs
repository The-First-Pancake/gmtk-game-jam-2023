using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindBehavior : MonoBehaviour
{
    public Vector3Int windDir;
    // Start is called before the first frame update
    void Start()
    {
        windDir = TileBehavior.NEIGHBOR_COORDS[UnityEngine.Random.Range(0, TileBehavior.NEIGHBOR_COORDS.Length - 1)];
        InvokeRepeating("WindChange", .5f, .5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void WindChange()
    {
        int index = Array.IndexOf(TileBehavior.NEIGHBOR_COORDS, windDir);
        int noise = UnityEngine.Random.Range(-1, 1);
        int newIndex = mod(index + noise, TileBehavior.NEIGHBOR_COORDS.Length); //Caps index at length
        Debug.Log(index.ToString() + noise.ToString() + newIndex.ToString());
        windDir = TileBehavior.NEIGHBOR_COORDS[newIndex];
    }
    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
}
