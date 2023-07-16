using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainCloud : MonoBehaviour
{

    public float offscreenAmount = 1.0f;
    public float movespeed = .25f;
    public Vector2 extinguishRange = new Vector2 (2, 1); // shape is in worldspace, so should generally have a ratio of 2:1


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(GameManager.instance.sceneHandler.IsTransitioning()) { return; }

        //Get windspeed
        Vector3 windDir = GameManager.instance.wind.GetWorldWindDir();
        //Move cloud
        transform.position += Time.deltaTime * windDir.normalized * movespeed;
        //check for off screen
        (Vector2 lowerLeft, Vector2 upperRight) = GameManager.instance.CameraBounds();

        //Wrapping
        if(transform.position.x < lowerLeft.x - offscreenAmount)
        {
            transform.position = new Vector3(upperRight.x + offscreenAmount, transform.position.y, transform.position.z); 
        }
        if (transform.position.x > upperRight.x + offscreenAmount)
        {
            transform.position = new Vector3(lowerLeft.x - offscreenAmount, transform.position.y, transform.position.z); 
        }
        if (transform.position.y < lowerLeft.y - offscreenAmount)
        {
            transform.position = new Vector3(transform.position.x, upperRight.y + offscreenAmount, transform.position.z); 
        }
        if (transform.position.y > upperRight.y + offscreenAmount)
        {
            transform.position = new Vector3(transform.position.x, lowerLeft.y - offscreenAmount, transform.position.z);
        }

        //Extinguish below
        for (float i = -extinguishRange.x/2; i <= extinguishRange.x/2; i += .5f)
        {
            for (float j = -extinguishRange.y/2; j <= extinguishRange.y/2; j += .5f)
            {
                //Debug.Log($"i: {i} j: {j}");
                Vector3Int cell = WorldMap.instance.grid.WorldToCell(transform.position + new Vector3(i, j, 0));
                List<TileBehavior> tiles = WorldMap.instance.GetAllTilesInCell(cell);
                foreach (TileBehavior tile in tiles) {
                    tile.Fire.extinguish();
                }
            }
        }
        

    }

    private bool IsObjectOffscreen(float threshold)
    {
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);

        if (viewportPosition.x < -threshold || viewportPosition.x > 1 + threshold ||
            viewportPosition.y < -threshold || viewportPosition.y > 1 + threshold)
        {
            
        }
        return false;
    }
}
