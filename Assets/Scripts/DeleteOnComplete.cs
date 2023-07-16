using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteOnComplete : MonoBehaviour
{
    public float timeUntilDelete = 10;
    public bool smart = false; //if it's smart it will not delete while an audio source or particle system are currently playing
    private float timeSpawned = 0;
    AudioSource audioSource;
    ParticleSystem particleSystem;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        particleSystem = GetComponent<ParticleSystem>();
        timeSpawned = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (smart && audioSource != null && audioSource.isPlaying){ return; }
        if (smart && particleSystem != null && particleSystem.isPlaying) { return; }
        if(Time.time < timeSpawned + timeUntilDelete) { return;}

        Destroy(gameObject);
    }
}
