using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainCloud : MonoBehaviour
{

    public float offscreenAmount = 1.0f;
    public float movespeed = .25f;
    public Vector2 extinguishRange = new Vector2 (2, 1);

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Get windspeed
        Vector3 windDir = GameManager.instance.wind.GetWorldWindDir();
        //Move cloud
        transform.position += Time.deltaTime * windDir.normalized * movespeed;
        //check for off screen

        (Vector2 lowerLeft, Vector2 upperRight) = GameManager.instance.CameraBounds();

        if(transform.position.x < lowerLeft.x - offscreenAmount || transform.position.x > upperRight.x + offscreenAmount ||
            transform.position.y < lowerLeft.y - offscreenAmount || transform.position.y > upperRight.y + offscreenAmount)
        {
            //wrap around the screen
            transform.position *= -1; //TODO I this needs to be more complicated for when the camera isnt centered, but will work for now
        }

        //Extinguish below
        for (float i = -extinguishRange.x/2; i <= extinguishRange.x/2; i++)
        {
            for (float j = -extinguishRange.y/2; j <= extinguishRange.y/2; j += .5f)
            {
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
