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

    [SerializeField]
    private GameObject lightningGFX;

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
                //Fire hasn't started yet. Tiem to do lightnig
                lr.enabled = false;
                if(Input.GetMouseButtonDown(0) && validShot){
                    StartCoroutine(LightningSequence(mouseTile));
                }
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

    IEnumerator LightningSequence(TileBehavior target){
        var newlightningGFX = Instantiate(lightningGFX);
        newlightningGFX.transform.position = target.WorldCoordinates;
        target.Fire.ignite();
        yield return new WaitForSeconds(.2f);
        Destroy(newlightningGFX);
        yield return null;
    }

    IEnumerator ShootProjectile(TileBehavior origin, TileBehavior target){
        state = PlayerState.cooldown;
        //Get path
        
        
        float dist = worldMap.grid.CellToWorld(target.IsoCoordinates - origin.IsoCoordinates).magnitude;
        Vector3Int isodir = (target.IsoCoordinates - origin.IsoCoordinates);
        Vector3 dir = worldMap.grid.CellToWorld(isodir);
        dir.Normalize();
        
        float launchAngle = Vector3.SignedAngle(dir, Vector3.right,Vector3.back) + 180;

        float timeToArrive = dist/projectileSpeed;
        
        GameObject newProjectile = Instantiate(projectile, origin.WorldCoordinates, Quaternion.Euler(new Vector3(0,0,launchAngle)));

        float launchTime = Time.time;
        while(Time.time < launchTime + timeToArrive){
            newProjectile.transform.position += -projectileSpeed*newProjectile.transform.right* Time.deltaTime;
            yield return null;
        }

        
        target.Fire.ignite();
        state = PlayerState.ready;

        newProjectile.GetComponent<SpriteRenderer>().enabled = false;
        newProjectile.GetComponentInChildren<ParticleSystem>().Stop();
        yield return new WaitForSeconds(5);
        Destroy(newProjectile);
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
