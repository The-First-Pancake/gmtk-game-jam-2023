using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState{
        ready,
        cooldown,
    }
    
    WorldMap worldMap;
    public GameObject gridIcon;
    LineRenderer lr;
    public int maxRange = 10;

    public Color validColor;
    public Color invalidColor;
    public float projectileSpeed = 2;
    public GameObject projectile;


    PlayerState state = PlayerState.ready;
    // Start is called before the first frame update
    void Start()
    {
        worldMap = WorldMap.instance;
        lr = gridIcon.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosWorld.z = 0;
        Vector3Int mousePosCell = worldMap.grid.WorldToCell(mousePosWorld);
        mousePosCell.z = 0;

        
        if(state == PlayerState.ready){
            gridIcon.SetActive(true);
            gridIcon.transform.position = worldMap.grid.GetCellCenterWorld(mousePosCell);

            TileBehavior mouseTile = WorldMap.instance.GetTopTile(mousePosCell);
            
            (TileBehavior closestFireTile, float dist) = getClosestFireTile(mousePosCell);

            //Check for invalid shot
            bool validShot = checkValidShot(mouseTile, dist);
            if(validShot){
                validShotGFX();
            } else {
                invalidShotGFX();
            }

            if(closestFireTile == null){
                invalidShotGFX();
                lr.enabled = false;
                return;
            }
            drawArc(closestFireTile.IsoCoordinates, mousePosCell);

            if(Input.GetMouseButtonDown(0) && validShot){
                StartCoroutine(ShootProjectile(closestFireTile, mouseTile));
            }
        }
        else if (state == PlayerState.cooldown){
            gridIcon.SetActive(false);
        }
    }

    IEnumerator ShootProjectile(TileBehavior origin, TileBehavior target){
        state = PlayerState.cooldown;
        //Get path
        GameObject newProjectile = Instantiate(projectile, origin.WorldCoordinates, Quaternion.identity);
        
        float dist = (target.IsoCoordinates - origin.IsoCoordinates).magnitude;
        float timeToArrive = dist/projectileSpeed;
        Debug.Log(dist);
        
        yield return new WaitForSeconds(timeToArrive);

        Debug.Log("arrived");
        Destroy(newProjectile);
        target.Fire.ignite();
        state = PlayerState.ready;
    }
    bool checkValidShot(TileBehavior tile, float dist){
        if(tile == null){return false;}
        bool burning = tile.Fire.state == FireBehaviour.burnState.burning;
        bool invalidTerrain = tile.Fire.flambilityScore == 0;
        bool tooFar = dist > maxRange;
        return !(burning || invalidTerrain || tooFar);
    }
    void invalidShotGFX(){
        lr.startColor = invalidColor;
        lr.endColor = invalidColor;
        gridIcon.GetComponent<SpriteRenderer>().color = invalidColor;
    }
    void validShotGFX(){
        lr.startColor = validColor;
        lr.endColor = validColor;
        gridIcon.GetComponent<SpriteRenderer>().color = validColor;
    }

    void drawArc(Vector3Int startCell, Vector3Int endCell){
        //TODO make parabola
        lr.enabled = true;
        Vector3[] pos = {worldMap.grid.GetCellCenterWorld(startCell),worldMap.grid.GetCellCenterWorld(endCell)};
        lr.SetPositions(pos);
    }
    (TileBehavior,float) getClosestFireTile(Vector3Int mousePosCell){
        List<TileBehavior> burningTiles = worldMap.GetAllBurningTiles();
        if (burningTiles.Count == 0){return (null,0);}

        TileBehavior closestTile = null;
        float closestFireDistance = Mathf.Infinity;
        foreach(TileBehavior burningTile in burningTiles){
            float dist = (burningTile.IsoCoordinates - mousePosCell).magnitude;
            if(dist < closestFireDistance){
                closestTile = burningTile;
                closestFireDistance = dist;
            }
        }
        return (closestTile,closestFireDistance);
    }
}
