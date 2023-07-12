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
    public SceneHandler sceneHandler;
    PlayerController playerController;
    WinLoseText winLoseText;

    Canvas UIcanvas;
    int totalBuildings = -1;
    int totalFire;

    public bool mute = false;
    public AudioSource peacfulMusic;
    public List<AudioSource> musicLevels;
    public List<int> musicLevelThresholds;

    public GameObject debugMessage;
    public GameObject debugTile;

    bool nextLevelCalled = false;
    bool restartLevelCalled = false;
    bool fireStarted = false;
    
    public UnityEvent spreadTick;
    float lastTickTime = 0;
    public static float spreadTickInterval = .25f;

    public List<TileBehavior> tilesToBeDeleted = new List<TileBehavior>();

    private void Awake()
    {
        instance = this;
        wind = gameObject.GetComponent<WindBehavior>();
        sceneHandler = gameObject.GetComponent<SceneHandler>();
        playerController = gameObject.GetComponent<PlayerController>();
        winLoseText = gameObject.GetComponent<WinLoseText>();
        UIcanvas = gameObject.GetComponentInChildren<Canvas>();
    }
    // Start is called before the first frame update
    void Start()
    {
        peacfulMusic.volume = 1;
        peacfulMusic.Play();
        foreach (AudioSource audio in musicLevels) {
            audio.Play();
        }
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

        UpdateMusic();
    }

    void checkKeybinds()
    {
        if (Input.GetKeyDown(KeyCode.R) && !sceneHandler.IsTransitioning()){
            restart();
        }

        if(Input.GetKeyDown(KeyCode.N) && !sceneHandler.IsTransitioning()){
            sceneHandler.NextLevel();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            mute = !mute;

            peacfulMusic.mute = mute;
            foreach(AudioSource track in musicLevels)
            {
                track.mute = mute;
            }
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

            restartLevelCalled = true;
            winLoseText.SetWinLoseText("LOSE");
            this.Invoke("lose", 1.25f);
        }
    }

    void UpdateMusic() {
        if (totalFire > 0 && !fireStarted) {
            fireStarted = true;
            peacfulMusic.volume = 0;
        }
        
        for (int i = 0; i < musicLevelThresholds.Count; i++) {
            if (totalFire > musicLevelThresholds[i]) {
                musicLevels[i].volume = 1;
            } else {
                float last_threshold = 0;
                if (i != 0) {
                    last_threshold = musicLevelThresholds[i-1];
                }
                float mapped_value = (totalFire - last_threshold) / (musicLevelThresholds[i] - last_threshold);
                musicLevels[i].volume = mapped_value;
            }
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
