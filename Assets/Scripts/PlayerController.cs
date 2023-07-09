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

    [HideInInspector]
    public bool usedLightning = false;

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

                if(usedLightning == true){
                    Debug.Log("YOU LOSE IDIOT");
                    GameManager.instance.lose();
                    return;
                }

                //othewise
                //Fire hasn't started yet. Tiem to do lightnig
                lr.enabled = false;
                if(Input.GetMouseButtonDown(0) && validShot){
                    StartCoroutine(LightningSequence(mouseTile));
                    usedLightning = true;
                }
                return;
            }
            drawArc(closestFireTile, mouseTile);

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
        Vector3 dir = worldMap.grid.CellToWorld(isodir).normalized;

        float launchAngle = Vector3.SignedAngle(dir, Vector3.right,Vector3.back) + 180;
        
        GameObject newProjectile = Instantiate(projectile, origin.WorldCoordinates, Quaternion.Euler(new Vector3(0,0,launchAngle)));
        GameObject newProjectileShadow = newProjectile.GetComponentInChildren<SpriteRenderer>().gameObject;

        (Vector3[] path, float pathLength) = getParabolaPath(origin, target);
        float timeToArrive = pathLength/projectileSpeed;

        float launchTime = Time.time;
        while(Time.time < launchTime + timeToArrive){
            float percentTravel = (Time.time-launchTime)/timeToArrive;
            newProjectileShadow.transform.position = origin.WorldCoordinates;
            int step = Mathf.FloorToInt(((float)path.Length * percentTravel));
            Vector3 currentPoint = path[step];
            Vector3 nextPoint = step+1<path.Length? path[step+1] : target.WorldCoordinates;
            newProjectile.transform.position = currentPoint;
            launchAngle = Vector3.SignedAngle(nextPoint - currentPoint, Vector3.right,Vector3.back) + 180;
            newProjectile.transform.rotation = Quaternion.Euler(new Vector3(0,0,launchAngle));
            yield return null;
        }

        
        target.Fire.ignite();
        state = PlayerState.ready;

        newProjectile.GetComponent<SpriteRenderer>().enabled = false;
        newProjectile.GetComponentInChildren<ParticleSystem>().Stop();
        yield return new WaitForSeconds(5);
        Destroy(newProjectile);
    }

    (Vector3[], float) getParabolaPath(TileBehavior origin, TileBehavior target){
        float resolution = 20;
        float dist = worldMap.grid.CellToWorld(target.IsoCoordinates - origin.IsoCoordinates).magnitude;
        Vector3Int isodir = (target.IsoCoordinates - origin.IsoCoordinates);
        Vector3 dir = worldMap.grid.CellToWorld(isodir).normalized;
        int steps = Mathf.FloorToInt(dist*resolution);
        steps = Mathf.Max(steps, 50); //minimum tep count is 50. Helps short range shots look good
        Vector3[] output = new Vector3[steps];
        float pathLength = 0;
        for(int step = 0; step < steps; step++){
            Vector3 newPoint = origin.WorldCoordinates + (dir * dist/steps)*step;
            newPoint.y += parabola(1.5f,dist,step*dist/steps);
            output[step] = newPoint;

            if(step>0){
                pathLength += (newPoint - output[step-1]).magnitude;
            }

            float parabola(float h, float l, float x){
                float a = h/(Mathf.Pow(l/2f,2));
                float c = l/2;
                return -(a*Mathf.Pow(x-c,2))+h;
            }
        }
        return (output, pathLength);
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

    void drawArc(TileBehavior origin, TileBehavior target){
        (Vector3[] path, float shitballs) = getParabolaPath(origin, target);
        lr.enabled = true;
        lr.positionCount = path.Length;
        lr.SetPositions(path);
        
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
