using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerBehavior : MonoBehaviour
{
    public enum VillagerState
    {
        IDLE = 0,
        ROAMING,
        ALERTED,
        GETTING_WATER,
        PUTTING_OUT_FIRE,
        PANICKING,
    }
    public VillagerState State = VillagerState.IDLE;
    private VillagerMovement movement;
    public TileBehavior CurrentTarget;
    public float StateMachineTickRateSeconds = 1f;

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<VillagerMovement>();
        InvokeRepeating("TickStateMachine", 0.1f, StateMachineTickRateSeconds);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TickStateMachine() {
        switch (State)
        {
            case VillagerState.IDLE:
                idleUpdate();
                break;
            case VillagerState.ROAMING:
                roamingUpdate();
                break;
            case VillagerState.ALERTED:
                alertedUpdate();
                break;
            case VillagerState.GETTING_WATER:
                gettingWaterUpdate();
                break;
            case VillagerState.PUTTING_OUT_FIRE:
                puttingOutFireUpdate();
                break;
            case VillagerState.PANICKING:
                panickingUpdate();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void panickingUpdate()
    {
        throw new NotImplementedException();
    }

    private void puttingOutFireUpdate()
    {
        throw new NotImplementedException();
    }

    private void gettingWaterUpdate()
    {
        throw new NotImplementedException();
    }

    private void alertedUpdate()
    {
        throw new NotImplementedException();
    }

    private void roamingUpdate()
    {
        if (movement.IsDoneMove()) {
            enterState(VillagerState.IDLE);
        }
    }

    private void idleUpdate()
    {
        enterState(VillagerState.ROAMING);
    }

    private void enterState(VillagerState new_state) {
        switch (new_state)
        {
            case VillagerState.IDLE:
                on_enterIdle();
                break;
            case VillagerState.ROAMING:
                on_enterRoaming();
                break;
            case VillagerState.ALERTED:
                on_enterAlerted();
                break;
            case VillagerState.GETTING_WATER:
                on_enterGettingWater();
                break;
            case VillagerState.PUTTING_OUT_FIRE:
                on_enterPuttingOutFire();
                break;
            case VillagerState.PANICKING:
                on_enterPanicking();
                break;
            default:
                throw new NotImplementedException();
        }
        State = new_state;
    }

    private void on_enterPanicking()
    {
        throw new NotImplementedException();
    }

    private void on_enterPuttingOutFire()
    {
        throw new NotImplementedException();
    }

    private void on_enterGettingWater()
    {
        throw new NotImplementedException();
    }

    private void on_enterAlerted()
    {
        throw new NotImplementedException();
    }

    private void on_enterRoaming()
    {
        List<TileBehavior> buildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING);
        int random_idx = UnityEngine.Random.Range(0, buildings.Count);
        TileBehavior targetBuilding = buildings[random_idx];
        TileBehavior target = targetBuilding;
        while (target.CanPath == TileBehavior.PathAble.BLOCKS_MOVEMENT) {
            Vector3Int near_building = new Vector3Int(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 2), 0);
            target = WorldMap.instance.GetTopTile(targetBuilding.IsoCoordinates + near_building);
        }
        CurrentTarget = target;
        movement.GoToTile(CurrentTarget);
    }

    private void on_enterIdle()
    {   
        // Nothing to do here... for now
        return;
    }
}
