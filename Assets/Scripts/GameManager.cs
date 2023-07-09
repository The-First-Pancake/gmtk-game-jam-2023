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

    bool nextLevelCalled = false;
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
        if (totalBuildings != 0 && remaingBuildings == 0 && !nextLevelCalled) {
            nextLevelCalled = true;
            
            sceneHandler.Invoke("NextLevel", 5);
        }

        if(Input.GetKeyDown(KeyCode.R)){
            restart();
        }
    }

    public void lose(){
        //TODO sad sound.
        restart();
    } 

    public void restart(){
        //Reload the level

    }
}
