using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireBehaviour : MonoBehaviour
{
    public enum burnState {unburnt, burning, burnt} //Burnt is currently unused

    [Range(0.0f, 1.0f)]
    public float flambilityScore = .5f; //Base chance of being spread to each second (assuming completly surrounded by fire)
    public float sustain = 5;
    private float timeStartedBurning = 0;
    public burnState state = burnState.unburnt;
    public GameObject firePrefab;
    private GameObject spawnedFire;

    private TileBehavior tileBehavior;

    static private float spreadInterval = .5f;

    [Header("Burnout Behavior")]
    [SerializeField]
    private bool destroyAfterBurnOut = false;
    [SerializeField]
    private Sprite[] burnoutSprites;

    // Start is called before the first frame update
    void Start()
    {
        tileBehavior = GetComponent<TileBehavior>();
        sustain *= Random.Range(0.9f, 1.1f); //Noise applied to sustain
        InvokeRepeating("onSpread", spreadInterval, spreadInterval);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == burnState.burning)
        {
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
            //check for ignition

            //Advanced coding and algorithms
            //TODO wind direction

            float igniteProbability = bruningNeighborsFactor() * flambilityScore * spreadInterval;
            
            if (igniteProbability > Random.Range(0f, 1f))
            {
                ignite();
            }
        }
    }
    [ContextMenu("Ignite")]
    public void ignite()
    {
        state = burnState.burning;
        timeStartedBurning = Time.time;
        spawnedFire = Instantiate(firePrefab);
        spawnedFire.transform.position = new Vector3(tileBehavior.WorldCoordinates.x, tileBehavior.WorldCoordinates.y - 0.5f, tileBehavior.WorldCoordinates.z);
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

        deleteParticles();

        state = burnState.unburnt;
    }

    float bruningNeighborsFactor()
    {
        List<TileBehavior> neighbors = tileBehavior.GetNeighbors();

        float burningNeighbors = 0;
        foreach (TileBehavior neighbor in neighbors)
        {
            if (neighbor.gameObject.GetComponent<FireBehaviour>())
            {
                if (neighbor.gameObject.GetComponent<FireBehaviour>().state == burnState.burning)
                {
                    Vector3 dirOfBurningNeighbor = neighbor.IsoCoordinates - tileBehavior.IsoCoordinates;
                    Vector3 windDir = GameManager.instance.wind.GetIsoWindDir();
                    float alignment = (Vector3.Angle(windDir , dirOfBurningNeighbor)/180); //should be 1 if aligned, 0 if not

                    burningNeighbors += alignment;
                }
            }
        }
        return burningNeighbors/8;
    }

    void deleteParticles()
    {
        var ps = spawnedFire.GetComponent<ParticleSystem>();
        ps.emissionRate = 0;
        Destroy(spawnedFire, ps.main.startLifetime.constant); //Destroy particle effect after its finished
    }
}
