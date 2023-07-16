using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManger : MonoBehaviour
{
    public bool mute = false;
    public AudioSource peacfulMusic;
    public List<AudioSource> musicLevels;
    public List<int> musicLevelThresholds;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;
        gameManager.OnFireStart.AddListener(OnFireStart);

        peacfulMusic.volume = 1;
        peacfulMusic.Play();
        foreach (AudioSource audio in musicLevels)
        {
            audio.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.M))
        {
            setMute(!mute);
        }

        UpdateMusic();
    }

    void OnFireStart()
    {
        peacfulMusic.volume = 0;
    }

    void UpdateMusic()
    {
        for (int i = 0; i < musicLevelThresholds.Count; i++)
        {
            if (gameManager.totalFire > musicLevelThresholds[i])
            {
                musicLevels[i].volume = 1;
            }
            else
            {
                float last_threshold = 0;
                if (i != 0)
                {
                    last_threshold = musicLevelThresholds[i - 1];
                }
                float mapped_value = (gameManager.totalFire - last_threshold) / (musicLevelThresholds[i] - last_threshold);
                musicLevels[i].volume = mapped_value;
            }
        }
    }

    void setMute(bool state)
    {
        mute = state;
        updateMute();
    }

    void updateMute()
    {
        peacfulMusic.mute = mute;
        foreach (AudioSource track in musicLevels)
        {
            track.mute = mute;
        }
    }
}
