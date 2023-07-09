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
    public AudioSource peacfulMusic;
    public List<AudioSource> musicLevels;
    public List<int> musicLevelThresholds;

    bool nextLevelCalled = false;
    bool fireStarted = false;
    private void Awake()
    {
        instance = this;
        wind = gameObject.GetComponent<WindBehavior>();
        sceneHandler = gameObject.GetComponent<SceneHandler>();
    }
    // Start is called before the first frame update
    void Start()
    {
        peacfulMusic.volume = 1;
        peacfulMusic.Play();
        foreach (AudioSource audio in musicLevels) {
            audio.Play();
        }
        totalBuildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING).Count;
    }

    // Update is called once per frame
    void Update()
    {
        int remaingBuildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING).Count;
        if (totalBuildings != 0 && remaingBuildings == 0 && !nextLevelCalled) {
            nextLevelCalled = true;
            
            sceneHandler.Invoke("NextLevel", 5);
        }

        if(Input.GetKeyDown(KeyCode.R)){
            restart();
        }
        totalFire = WorldMap.instance.GetAllBurningTiles().Count;
        UpdateMusic();
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

    public void lose(){
        //TODO sad sound.
        restart();
    } 

    public void restart(){
        sceneHandler.Restart();
    }
}
