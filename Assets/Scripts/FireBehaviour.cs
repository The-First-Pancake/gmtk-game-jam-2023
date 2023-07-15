using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class FireBehaviour : MonoBehaviour
{
    public enum burnState {unburnt, burning, burnt} //Burnt is currently unused

    public float flambilityScore = .5f; //Base chance of being spread to each second (assuming completly surrounded by fire)
    public float sustain = 5;
    public float dangerRating= 0;
    public float timeStartedBurning = 0;
    public burnState state = burnState.unburnt;
    public GameObject firePrefab;
    private GameObject spawnedFire;

    private TileBehavior tileBehavior;

    public bool cannotBeSpreadFrom = false;

    [Header("Burnout Behavior")]
    [SerializeField]
    private bool destroyAfterBurnOut = false;
    [SerializeField]
    private Sprite[] burnoutSprites;
    [SerializeField]
    private IsometricRuleTile[] burnoutTiles;
    [SerializeField]
    private GameObject[] burnoutGameObjects;
    [SerializeField]
    private GameObject burnoutSpriteObjectPrefab;

    

    // Start is called before the first frame update
    void Start()
    {
        tileBehavior = GetComponent<TileBehavior>();
        sustain *= Random.Range(0.9f, 1.1f); //Noise applied to sustain
        //GameManager.instance.spreadTick.AddListener(new UnityAction(onSpread));

        //InvokeRepeating("onSpread", GameManager.spreadTickInterval, GameManager.spreadTickInterval);
        if(burnoutTiles.Length > 0 && destroyAfterBurnOut == true)
        {
            Debug.Log($"Trying to spawn a Tile ({gameObject.name}) which is replaced by a burnout tile (which automatically destroys it after burnout for some unidentified reason) that also has 'destroy after burnout' checked. this double destroy break things, so I'll set destroyAfterBurnout to false for you");
            destroyAfterBurnOut=false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (state == burnState.burning) { 
            updateDanger();
            //Tickup running burn time, check if we've passed sustain
            
            if (timeStartedBurning + sustain <= Time.time)
            {
                burnComplete();
            }
        }
    }

    public void onSpread()
    {
        if (WorldMap.instance.GetTopTile(tileBehavior.IsoCoordinates) != tileBehavior) { return; } //Do not try to burn if there is a tile above

        if (state == burnState.unburnt){
            //Advanced coding and algorithms
            float igniteProbability = tickRateProbabilityCompensation(burningNeighborsFactor() * flambilityScore) ;
            
            if (igniteProbability > Random.Range(0f, 1f))
            {
                ignite();
            }
        }

        float tickRateProbabilityCompensation(float prob)
        {
            if (prob == 0) { return 0; } //This function is not defined at 0, but handing 0 as 0 makes the most sense
            float spreadsPerSec = (1 / GameManager.spreadTickInterval);
            float output = 1f - Mathf.Pow((1f - prob), (1f / spreadsPerSec));
            return output;
        }
    }



    private void updateDanger()
    {
        List<TileBehavior> buildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING);
        float closestBuildingDistance = float.PositiveInfinity;
        foreach (TileBehavior building in buildings) {
            float buildingDistance = (building.WorldCoordinates - transform.position).magnitude;
            closestBuildingDistance = Mathf.Min(buildingDistance, closestBuildingDistance);
        }
        dangerRating = 1/closestBuildingDistance;
    }

    [ContextMenu("Ignite")]
    public void ignite()
    {
        if(state == burnState.burning){return;}
        if(flambilityScore == 0){return;}
        state = burnState.burning;
        timeStartedBurning = Time.time;
        spawnedFire = Instantiate(firePrefab);
        spawnedFire.transform.position = tileBehavior.WorldCoordinates;
        WorldMap.instance.refreshTile(tileBehavior);
    }
    public void extinguish()
    {
        if(state != burnState.burning) { return; }
        state = burnState.unburnt;


        deleteParticles();
        WorldMap.instance.refreshTile(tileBehavior);
    }

    public void burnComplete()
    {
        if (state != burnState.burning) { Debug.Log("What the hell oh my god"); return; }
        state = burnState.unburnt;
        if (burnoutSprites.Length > 0)
        {
            Vector3 noisyPos = transform.position + new Vector3(Random.Range(-.10f, .10f), Random.Range(-.10f, -.10f), 0);
            GameObject newSpawned = Instantiate(burnoutSpriteObjectPrefab, noisyPos, transform.rotation);
            newSpawned.GetComponent<SpriteRenderer>().sprite = burnoutSprites[Random.Range(0, burnoutSprites.Length)];
        }
        if (burnoutTiles.Length > 0) {
            tileBehavior.tilemap.SetTile(tileBehavior.IsoCoordinates, burnoutTiles[Random.Range(0, burnoutSprites.Length)]);
        }
        if (burnoutGameObjects.Length > 0) {
            var newGO = Instantiate(burnoutGameObjects[Random.Range(0, burnoutSprites.Length)]);
            newGO.transform.position = tileBehavior.WorldCoordinates;
        }
        WorldMap.instance.refreshTile(tileBehavior);
        if (destroyAfterBurnOut)
        {
            tileBehavior.DeleteTile();
        }
        deleteParticles();

    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        WorldMap.instance.refreshTile(tileBehavior);
    }

    float burningNeighborsFactor() // value between 0 and 2 that will modify the likelyhood of neighboring tiles catching on fire
    {
        List<TileBehavior> neighbors = tileBehavior.GetNeighbors();

        float burningNeighbors = 0;
        foreach (TileBehavior neighbor in neighbors)
        {
            if (neighbor.gameObject.GetComponent<FireBehaviour>())
            {
                if (neighbor.gameObject.GetComponent<FireBehaviour>().state == burnState.burning)
                {
                    Vector3 windDir = GameManager.instance.wind.GetIsoWindDir();
                    Vector3 dirOfBurningNeighbor = neighbor.IsoCoordinates - tileBehavior.IsoCoordinates;
                    burningNeighbors += neighborWindModifier(dirOfBurningNeighbor, windDir);
                }
            }
        }
        return burningNeighbors/4 * 2f;
    }
    float neighborWindModifier(Vector3 neighborDir, Vector3 windDir){
        return (Vector3.Angle(windDir , neighborDir)/180); //should be 1 if aligned, 0 if not
    }

    void deleteParticles()
    {
        if(spawnedFire == null){return;}
        var ps = spawnedFire.GetComponent<ParticleSystem>();
        ps.Stop();
        Destroy(spawnedFire, ps.main.startLifetime.constant); //Destroy particle effect after its finished
    }
}
