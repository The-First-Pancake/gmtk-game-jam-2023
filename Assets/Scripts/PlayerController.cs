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
    public float postLandingCooldown = 0f;
    public GameObject projectile;

    [SerializeField]
    private GameObject lightningGFX;

    SceneHandler SceneHandler;

    [HideInInspector]
    public bool usedLightning = false;

    public PlayerState state = PlayerState.ready;

    public AudioClip fireBallIgniteSound;
    // Start is called before the first frame update
    void Start()
    {
        worldMap = WorldMap.instance;
        lr = gridIcon.GetComponent<LineRenderer>();
        SceneHandler = GetComponent<SceneHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int mousePosCell = worldMap.grid.WorldToCell(mousePosWorld);
        mousePosCell.z = 0;
        
        if (SceneHandler.IsTransitioning()) {
            gridIcon.SetActive(false);
        }
        else if(state == PlayerState.ready){
            gridIcon.SetActive(true);
            gridIcon.transform.position =  Vector3.ClampMagnitude(worldMap.grid.GetCellCenterWorld(mousePosCell), 150);

            TileBehavior mouseTile = WorldMap.instance.GetTopTile(mousePosCell);
            
            (TileBehavior closestFireTile, float dist) = getClosestFireTile(mousePosCell);

            //Check for invalid shot
            bool validShot = checkValidShot(mouseTile, dist, closestFireTile);
            if(validShot){
                validShotGFX();
            } else {
                invalidShotGFX();
            }

            if(closestFireTile == null){
                lr.enabled = false;

                //othewise
                //Fire hasn't started yet. Tiem to do lightnig
                
                if(Input.GetMouseButtonDown(0) && validShot && !usedLightning){
                    StartCoroutine(LightningSequence(mouseTile));
                    usedLightning = true;
                }
                return;
            }
            
            if(mouseTile != closestFireTile){
                drawArc(closestFireTile, mouseTile);
            } else {
                lr.enabled = false; 
            }

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
        yield return new WaitForSeconds(5f);
        Destroy(newlightningGFX);
        yield return null;
    }

    IEnumerator ShootProjectile(TileBehavior origin, TileBehavior target){
        state = PlayerState.cooldown;

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

        GameManager.instance.audioManager.PlaySound(fireBallIgniteSound,.4f);
        target.Fire.ignite();

        //Turn off projectile visuals
        newProjectile.GetComponent<SpriteRenderer>().enabled = false;
        newProjectile.GetComponentInChildren<ParticleSystem>().Stop();

        yield return new WaitForSeconds(postLandingCooldown);
        state = PlayerState.ready;
        yield return new WaitForSeconds(5);
        Destroy(newProjectile); //delete projectile after the visuals were able to cool down
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

    bool checkValidShot(TileBehavior tile, float dist, TileBehavior closestFireTile){
        if(tile == null){return false;}
        bool burning = tile.Fire.state == FireBehaviour.burnState.burning;
        bool invalidTerrain = tile.Fire.flambilityScore == 0;
        bool tooFar = dist > maxRange;
        bool alreadyUsedLightning = !closestFireTile && usedLightning;
        return !(burning || invalidTerrain || tooFar || alreadyUsedLightning);
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
        //if(Time.timeSinceLevelLoad < .1f) { return; } //TODO make this less bad (this is here b/c sometimes on the first frame of a level it'll try to draw the parab and fail). This is supposed to be gated by the isTransitioning function, but that don't always work.
        //On second thought maybe its on the last frame of the level, since this fix didn't stop the error lol
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
            if(burningTile.Fire.cannotBeSpreadFrom == true){
                continue;
            }
            if(dist < closestFireDistance){
                closestTile = burningTile;
                closestFireDistance = dist;
            }
        }
        return (closestTile,closestFireDistance);
    }
}
