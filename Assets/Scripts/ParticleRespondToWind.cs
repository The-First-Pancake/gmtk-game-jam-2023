using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleRespondToWind : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private float WindForceMultiplier = .15f;
    private Vector2 startingSmokeDirection;
    // Start is called before the first frame update
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        ParticleSystem.ForceOverLifetimeModule psfolt = _particleSystem.forceOverLifetime;

        startingSmokeDirection = new Vector2(psfolt.x.constant, psfolt.y.constant);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 windDir = GameManager.instance.wind.GetWorldWindDir();
        ParticleSystem.ForceOverLifetimeModule psfolt = _particleSystem.forceOverLifetime;
        psfolt.x = WindForceMultiplier * windDir.x + startingSmokeDirection.x;
        psfolt.y = WindForceMultiplier * windDir.y + startingSmokeDirection.y;

    }
}
