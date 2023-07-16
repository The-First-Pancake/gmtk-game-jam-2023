using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainCloud : MonoBehaviour
{

    public float movespeed = .25f;
    public Vector2 extinguishRange = new Vector2 (2, 1); // shape is in worldspace, so should generally have a ratio of 2:1
    [HideInInspector]
    public bool isCopy = false;

    void Start()
    {

        //We make 3 copies so that it wraps perfectly around the edges of the screen
        if(isCopy == false)
        {
            (Vector2 lowerLeft, Vector2 upperRight) = GameManager.instance.CameraBounds();

            float screenWidth = upperRight.x - lowerLeft.x;
            float screenHeight = upperRight.y - lowerLeft.y;

            var copy1 = Instantiate(gameObject, transform.position + new Vector3(screenWidth, 0,0), Quaternion.identity);
            copy1.GetComponent<RainCloud>().isCopy = true;
            var copy2 = Instantiate(gameObject, transform.position + new Vector3(screenWidth, screenHeight, 0), Quaternion.identity);
            copy2.GetComponent<RainCloud>().isCopy = true;
            var copy3 = Instantiate(gameObject, transform.position + new Vector3(0, screenHeight, 0), Quaternion.identity);
            copy3.GetComponent<RainCloud>().isCopy = true;
        }
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

        float screenWidth = upperRight.x - lowerLeft.x; 
        float screenHeight = upperRight.y - lowerLeft.y;

        //Wrapping
        if(transform.position.x < lowerLeft.x - screenWidth/2)
        {
            float distancePastBoundry = transform.position.x - (lowerLeft.x - screenWidth / 2);
            transform.position = new Vector3(upperRight.x + screenWidth / 2 + distancePastBoundry, transform.position.y, transform.position.z); 
        }
        if (transform.position.x > upperRight.x + screenWidth/2)
        {
            float distancePastBoundry = transform.position.x - (upperRight.x + screenWidth / 2);
            transform.position = new Vector3(lowerLeft.x - screenWidth / 2 + distancePastBoundry, transform.position.y, transform.position.z); 
        }
        if (transform.position.y < lowerLeft.y - screenHeight/2)
        {
            float distancePastBoundry = transform.position.y - (lowerLeft.y - screenHeight / 2);
            transform.position = new Vector3(transform.position.x, upperRight.y + screenHeight / 2 - distancePastBoundry, transform.position.z); 
        }
        if (transform.position.y > upperRight.y + screenHeight/2)
        {
            float distancePastBoundry = transform.position.y - (upperRight.y + screenHeight / 2);
            transform.position = new Vector3(transform.position.x, lowerLeft.y - screenHeight / 2 + distancePastBoundry, transform.position.z);
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
