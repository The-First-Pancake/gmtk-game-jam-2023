using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class FireBehaviour : MonoBehaviour
{
    public enum burnState {unburnt, burning, burnt} //Burnt is currently unused

    public float flambilityScore = .5f; //Base chance of being spread to each second (assuming completly surrounded by fire)
    public float sustain = 5;
    public float health;
    public float dangerRating= 0;
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


    [Header("Effects")]
    public GameObject extinguishEffect;
    

    // Start is called before the first frame update
    void Start()
    {
        tileBehavior = GetComponent<TileBehavior>();
        health = sustain;
        health *= Random.Range(0.9f, 1.1f); //Noise applied to starting health
        GameManager.instance.spreadTick.AddListener(new UnityAction(onSpread));

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

            //tick down health
            health -= Time.deltaTime;
            if (health <= 0)
            {
                burnComplete();
            }
        }
    }

    void onSpread()
    {
        if (state != burnState.unburnt) { return; }
        if(flambilityScore <= 0) { return; }
        if (WorldMap.instance.GetTopTile(tileBehavior.IsoCoordinates) != tileBehavior) { return; } //IDo not try to burn if there is a tile above

        //Advanced coding and algorithms
        float burningNeughtborsFactor = BurningNeighborsFactor();
        if(burningNeughtborsFactor <= 0) { return; }

        float igniteProbability = tickRateProbabilityCompensation(burningNeughtborsFactor * flambilityScore) ;
            
        if (igniteProbability > Random.Range(0f, 1f))
        {
            ignite();
        }
        
    }

    float tickRateProbabilityCompensation(float prob){

        if(prob == 0){return 0;}
        float spreadsPerSec = (1/GameManager.spreadTickInterval);
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
        if(state == burnState.burning){return;}
        if(flambilityScore == 0){return;}
        state = burnState.burning;

        //Particle Effect
        spawnedFire = Instantiate(firePrefab);
        spawnedFire.transform.position = tileBehavior.WorldCoordinates;
        //spawnedFire.transform.parent = this.transform; Removed so the particles can hang out for a bit after the gameobject is destroyed.
    }
    public void extinguish()
    {
        if(state != burnState.burning) { return; }
        state = burnState.unburnt;
        Destroy(spawnedFire);
        var ee = Instantiate(extinguishEffect);
        ee.transform.position = tileBehavior.WorldCoordinates;
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
        if (destroyAfterBurnOut)
        {
            tileBehavior.DeleteTile();
        }
        deleteParticles();


    }

    float BurningNeighborsFactor() // value between 0 and 2 that will modify the likelyhood of neighboring tiles catching on fire
    {
        List<TileBehavior> neighbors = tileBehavior.GetNeighbors();

        float burningNeighbors = 0;
        Vector3 windDir = GameManager.instance.wind.GetIsoWindDir();
        foreach (TileBehavior neighbor in neighbors)
        {
            if (neighbor.Fire)
            {
                if (neighbor.Fire.state == burnState.burning)
                {
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
        if(spawnedFire == null){return;}
        var ps = spawnedFire.GetComponent<ParticleSystem>();
        ps.Stop();
        Destroy(spawnedFire, ps.main.startLifetime.constant); //Destroy particle effect after its finished
    }
}
