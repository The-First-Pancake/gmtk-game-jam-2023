using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManger : MonoBehaviour
{
    [HideInInspector]
    public bool mute = false;

    [Header("Music")]
    public AudioSource peacfulMusic;
    public List<AudioSource> musicLevels;
    public List<int> musicLevelThresholds;

    [Header("SoundFX")]
    public List<AudioClip> miscSounds;
    private AudioSource catchallAudioSource;
    public AudioSource fireCracklingSounds;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.instance;
        gameManager.OnFireStart.AddListener(OnFireStart);

        catchallAudioSource = GetComponent<AudioSource>();  

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

        fireCracklingSounds.volume = Mathf.Clamp(gameManager.totalFire / 100f, 0, .75f);

        UpdateMusic();
    }

    public void PlaySound(AudioClip clip, float volume = 1)
    {
        catchallAudioSource.PlayOneShot(clip, volume);
    }

    public void PlaySound(string clipname, float volume = 1)
    {
        AudioClip clip = miscSounds.Find(x => x.name == clipname);
        if (clip == null){Debug.LogWarning($"Attempted to find clip {clipname} and was unable to. Make sure clip exists in the audioManager miscClips list"); return; }

        catchallAudioSource.PlayOneShot(clip, volume);
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
        catchallAudioSource.mute = mute;
        peacfulMusic.mute = mute;
        foreach (AudioSource track in musicLevels)
        {
            track.mute = mute;
        }
    }
}
