using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireBehaviour : MonoBehaviour
{
    public enum burnState {unburnt, burning, burnt} //Burnt is currently unused

    [Range(0.0f, 1.0f)]
    public float flambilityScore = .5f; 
    public float sustain = 5;
    private float timeStartedBurning = 0;
    public burnState state = burnState.unburnt;
    public GameObject firePrefab;
    private GameObject spawnedFire;

    private TileBehavior tileBehavior;

    

    // Start is called before the first frame update
    void Start()
    {
        tileBehavior = GetComponent<TileBehavior>();

        InvokeRepeating("onSpread", .25f, .25f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void onSpread()
    {
        if(WorldMap.instance.GetTopTile(tileBehavior.IsoCoordinates) != tileBehavior) { return; } //IDo not try to burn if there is a tile above

        if (state == burnState.unburnt){
            //check for ignition

            //Advanced coding and algorithms
            //TODO wind direction

            float igniteProbability = (float)burningNeighborsCount() / 8f * flambilityScore;
            
            if (igniteProbability > Random.Range(0f, 1f))
            {
                ignite();
            }
        }
        else if(state == burnState.burning){
            //Tickup running burn time, check if we've passed sustain
            if(Time.time > timeStartedBurning + sustain) {
                extinguish();
            }
        }
        else{
            //not currently used

        }

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
        //TODO if we're grass, turn burnt GFX, then reset. 
        //TODO otherwise, spawn some rubble and delete
        Destroy(spawnedFire);
        state = burnState.unburnt;
        Debug.Log("Extinguish");
    }

    int burningNeighborsCount()
    {
        List<TileBehavior> neighbors = tileBehavior.GetNeighbors();

        int burningNeighbors = 0;
        foreach (TileBehavior neighbor in neighbors)
        {
            if (neighbor.gameObject.GetComponent<FireBehaviour>())
            {
                if (neighbor.gameObject.GetComponent<FireBehaviour>().state == burnState.burning)
                {
                    burningNeighbors += 1;
                }
            }
        }
        return burningNeighbors;
    }

}