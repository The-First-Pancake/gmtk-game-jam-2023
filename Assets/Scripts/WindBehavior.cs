using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindBehavior : MonoBehaviour
{
    public float windAngle;
    public float windSpeed;

    public float playerWindPush = 60; //Maximum amount thhe player can push on the wind direction

    //wind pull is the measure of how hard the wind will try to change direction on its own
    public float windPullMagnitude = 30; //Maximum Degrees Per Second of random pull
    private float windPullDirection;  //Randomly determined direction/multiplier of wind pull
    

    // Start is called before the first frame update
    void Start()
    {
        windAngle = 90;//UnityEngine.Random.Range(0f, 360f);
        InvokeRepeating("WindPullChange", .5f, .5f);
    }

    // Update is called once per frame
    void Update()
    {

        windAngle += windPullDirection * windPullMagnitude * Time.deltaTime;

        windAngle += -Input.GetAxisRaw("Horizontal") * playerWindPush * Time.deltaTime;

    }

    void WindPullChange()
    {
        windPullDirection = UnityEngine.Random.Range(-1f, 1f);
    }
    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    public Vector3 GetWorldWindDir()
    {
        var grid = WorldMap.instance.grid;

        Vector3 GetCellCenterWorld(Vector3 position)
        {
            return grid.LocalToWorld(grid.CellToLocalInterpolated(position + grid.GetLayoutCellCenter()));
        }

        Vector2 worldCoordinatesOfIsoDir = GetCellCenterWorld(GetIsoWindDir());
        Vector2 gridCenterCoordinates = grid.GetCellCenterWorld(Vector3Int.zero);
        Vector2 postionAdjustedWorldWindDir = worldCoordinatesOfIsoDir - gridCenterCoordinates;
        return postionAdjustedWorldWindDir.normalized;
    }
    public Vector3 GetIsoWindDir()
    {
        float radians = windAngle * Mathf.Deg2Rad;
        return new Vector3((float)Math.Cos(radians), (float)Math.Sin(radians), 0);
    }

    public Vector3Int getIsoWindRiInt()
    {
        return new Vector3Int(Mathf.RoundToInt(GetIsoWindDir().x), Mathf.RoundToInt(GetIsoWindDir().y), 0);
    }

}
