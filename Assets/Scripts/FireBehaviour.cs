using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireBehaviour : MonoBehaviour
{
    public enum burnState {unburnt, burning, burnt} //Burnt is currently unused

    public float flambilityScore = .5f; //Base chance of being spread to each second (assuming completly surrounded by fire)
    public float sustain = 5;
    public float dangerRating= 0;
    private float timeStartedBurning = 0;
    public burnState state = burnState.unburnt;
    public GameObject firePrefab;
    private GameObject spawnedFire;

    private TileBehavior tileBehavior;

    static private float spreadInterval = .25f;

    [Header("Burnout Behavior")]
    [SerializeField]
    private bool destroyAfterBurnOut = false;
    [SerializeField]
    private Sprite[] burnoutSprites;
    [SerializeField]
    private IsometricRuleTile[] burnoutTiles;

    // Start is called before the first frame update
    void Start()
    {
        tileBehavior = GetComponent<TileBehavior>();
        sustain *= Random.Range(0.9f, 1.1f); //Noise applied to sustain
        InvokeRepeating("onSpread", Random.Range(0,spreadInterval), spreadInterval);


    }

    // Update is called once per frame
    void Update()
    {
        if (state == burnState.burning)
        {
            updateDanger();
            //Tickup running burn time, check if we've passed sustain
            if (Time.time > timeStartedBurning + sustain)
            {
                burnComplete();
            }
        }
    }

    void onSpread()
    {
        if(WorldMap.instance.GetTopTile(tileBehavior.IsoCoordinates) != tileBehavior) { return; } //IDo not try to burn if there is a tile above

        if (state == burnState.unburnt){
            //Advanced coding and algorithms

            float igniteProbability = tickRateProbabilityCompensation(bruningNeighborsFactor() * flambilityScore) ;
            
            if (igniteProbability > Random.Range(0f, 1f))
            {
                ignite();
            }
        }
    }

    float tickRateProbabilityCompensation(float prob){

        if(prob == 0){return 0;}
        float spreadsPerSec = (1/spreadInterval);
        float output = 1f-Mathf.Pow((1f-prob),(1f/spreadsPerSec));
        return output;
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
        state = burnState.burning;
        timeStartedBurning = Time.time;
        spawnedFire = Instantiate(firePrefab);
        spawnedFire.transform.position = tileBehavior.WorldCoordinates;
    }
    public void extinguish()
    {
        state = burnState.unburnt;
        sustain = (timeStartedBurning + sustain) - Time.time;
        deleteParticles();

    }

    public void burnComplete()
    {
        if(state != burnState.burning) { Debug.Log("What the hell oh my god"); return; }

        if (destroyAfterBurnOut)
        {
            if (burnoutSprites.Length > 0)
            {
                Vector3 noisyPos = transform.position + new Vector3(Random.Range(-.10f, .10f), Random.Range(-.10f, -.10f), 0);
                GameObject newSpawned = Instantiate((Resources.Load("Burnout Sprite Prefab") as GameObject), noisyPos, transform.rotation);
                newSpawned.GetComponent<SpriteRenderer>().sprite = burnoutSprites[Random.Range(0, burnoutSprites.Length - 1)];
            }
            tileBehavior.DeleteTile();
        }
        if (burnoutTiles.Length > -0) {
            tileBehavior.tilemap.SetTile(tileBehavior.IsoCoordinates, burnoutTiles[Random.Range(0, burnoutSprites.Length - 1)]);
        }

        deleteParticles();

        state = burnState.unburnt;
    }

    float bruningNeighborsFactor() // value between 0 and 2 that will modify the likelyhood of neighboring tiles catching on fire
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
        return burningNeighbors/4 * 1.5f;
    }
    float neighborWindModifier(Vector3 neighborDir, Vector3 windDir){
        return (Vector3.Angle(windDir , neighborDir)/180); //should be 1 if aligned, 0 if not
    }

    void deleteParticles()
    {
        var ps = spawnedFire.GetComponent<ParticleSystem>();
        ps.emissionRate = 0;
        Destroy(spawnedFire, ps.main.startLifetime.constant); //Destroy particle effect after its finished
    }
}
