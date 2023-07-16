using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [HideInInspector]
    public WindBehavior wind;
    [HideInInspector]
    public SceneHandler sceneHandler;
    [HideInInspector]
    public AudioManger audioManager;
    PlayerController playerController;
    WinLoseText winLoseText;

    Canvas UIcanvas;
    int totalBuildings = -1;
    [HideInInspector]
    public int totalFire;
    [HideInInspector]
    public UnityEvent OnFireStart;

    public GameObject debugMessage;
    public GameObject debugTile;

    bool nextLevelCalled = false;
    bool restartLevelCalled = false;
    bool fireStarted = false;

    [HideInInspector]
    public UnityEvent spreadTick;
    float lastTickTime = 0;
    public static float spreadTickInterval = .25f;

    private float timeSinceLoss = 0;


    private void Awake()
    {
        instance = this;
        wind = gameObject.GetComponent<WindBehavior>();
        sceneHandler = gameObject.GetComponent<SceneHandler>();
        playerController = gameObject.GetComponent<PlayerController>();
        winLoseText = gameObject.GetComponent<WinLoseText>();
        UIcanvas = gameObject.GetComponentInChildren<Canvas>();
        audioManager = gameObject.GetComponentInChildren<AudioManger>();
    }
    // Start is called before the first frame update
    void Start()
    {
        UIcanvas.worldCamera = Camera.main;
    }


    // Update is called once per frame
    void Update()
    {
        if(totalBuildings == -1 && Time.timeSinceLevelLoad > 1)//TODO make less jank
        {
            totalBuildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING).Count;
        }

        //Trigger the Fire Ticks
        if(lastTickTime + spreadTickInterval < Time.time) {
            lastTickTime = Time.time;
            spreadTick.Invoke();
        }

        checkWinLoseConditions();

        checkKeybinds();

        totalFire = WorldMap.instance.GetAllBurningTiles().Count;

        if (totalFire > 0 && !fireStarted)
        {
            fireStarted = true;
            OnFireStart.Invoke();
        }
    }

    void checkKeybinds()
    {
        if (Input.GetKeyDown(KeyCode.R) && !sceneHandler.IsTransitioning()){
            restart();
        }

        if(Input.GetKeyDown(KeyCode.N) && !sceneHandler.IsTransitioning()){
            sceneHandler.NextLevel();
        }


        if (Input.GetKeyDown(KeyCode.Escape) && !sceneHandler.IsTransitioning()){
            Application.Quit();
        }
    }

    void checkWinLoseConditions()
    {
        //TODO make better. Don't check wins or losses in the first second of the level
        if(Time.timeSinceLevelLoad < 1) { return; }

        int remaingBuildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING).Count;

        if (totalBuildings != 0 && remaingBuildings == 0 && !nextLevelCalled)
        {
            nextLevelCalled = true;
            winLoseText.SetWinLoseText("WIN");
            sceneHandler.Invoke("NextLevel", 1.25f);
        }
        else if (WorldMap.instance.GetAllBurningTiles().Count == 0 &&
                    playerController.usedLightning &&
                    !restartLevelCalled &&
                    !nextLevelCalled &&
                    playerController.state != PlayerController.PlayerState.cooldown)
        {
            timeSinceLoss += Time.deltaTime; //We must be losing for a half second before we actually call it a loss. Fixes explosion insta-loss
            if (timeSinceLoss > .5f)
            {
                restartLevelCalled = true;
                winLoseText.SetWinLoseText("LOSE");
                this.Invoke("lose", 1.25f);
            }
        }
        else
        {
            timeSinceLoss = 0;
        }
    }


    public void spawnDebugMSG(string message, Vector3 spawnPt, float noise, float lifetime = 0)
    {
        GameObject newMsg = Instantiate(debugMessage);
        Vector3 noiseVec = new Vector3(Random.Range(-noise,noise), Random.Range(-noise, noise), 0);
        newMsg.transform.position = spawnPt + noiseVec;
        newMsg.GetComponent<TextMeshPro>().text = message;
        if(lifetime != 0) {
            Destroy(newMsg, lifetime);
        }
        
    }
    public void spawnDebugtile(Color color, Vector3 spawnPt, float noise)
    {
        GameObject newTile = Instantiate(debugTile);
        Vector3 noiseVec = new Vector3(Random.Range(-noise, noise), Random.Range(-noise, noise), 0);
        newTile.transform.position = spawnPt + noiseVec;
        newTile.GetComponent<SpriteRenderer>().color = color;
        
    }
    public void lose(){
        //TODO sad sound.
        restart();
    } 

    public void restart(){
        sceneHandler.Restart();
    }

    public (Vector2, Vector2) CameraBounds()
    {
        return (Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)), Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)));

    }
}
