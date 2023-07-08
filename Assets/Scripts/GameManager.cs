using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public WindBehavior wind;
    SceneHandler sceneHandler;
    int totalBuildings;
    int totalFire;
    private void Awake()
    {
        instance = this;
        wind = gameObject.GetComponent<WindBehavior>();
        sceneHandler = gameObject.GetComponent<SceneHandler>();
    }
    // Start is called before the first frame update
    void Start()
    {
        totalBuildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING).Count;
    }

    // Update is called once per frame
    void Update()
    {
        int remaingBuildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING).Count;
        Debug.Log(remaingBuildings);
        if (totalBuildings != 0 && remaingBuildings == 0) {
            sceneHandler.NextLevel();
        }
    }
}
