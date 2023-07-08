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
    public TileBehavior CurrentFire;
    public float StateMachineTickRateSeconds = 1f;
    public int FireSenseDistanceSquares = 5;

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
        // Check if we can put out any fires
        TileBehavior currentTile = movement.GetCurrentTile();
        foreach (TileBehavior neighbor in currentTile.GetNeighbors()) {
            if (neighbor.Fire.state == FireBehaviour.burnState.burning) {
                neighbor.Fire.extinguish();
                enterState(VillagerState.ALERTED);
            }
        }

        // Check if we're still heading to the highest danger fire we can see
        TileBehavior dangerFire = GetHighestDangerFire();
        if (dangerFire) {
            if (CurrentFire == null || dangerFire.Fire.dangerRating > CurrentFire.Fire.dangerRating) {
                CurrentTarget = dangerFire;
                movement.GoToNeighborOf(dangerFire);
            }
        }
    }

    private void gettingWaterUpdate()
    {
        if (movement.IsDoneMove()) {
            enterState(VillagerState.PUTTING_OUT_FIRE);
        } 
    }

    private void alertedUpdate()
    {   
        enterState(VillagerState.GETTING_WATER);
    }

    private void roamingUpdate()
    {
        if (movement.IsDoneMove()) {
            enterState(VillagerState.IDLE);
        } else if (CanSeeFire()) {
            enterState(VillagerState.ALERTED);
        }
    }

    private bool CanSeeFire() {
        for (int i = -FireSenseDistanceSquares; i < FireSenseDistanceSquares; i++) {
            for (int j = -FireSenseDistanceSquares; i < FireSenseDistanceSquares; i++) {
                Vector3Int offset = new Vector3Int(i, j, 0);
                TileBehavior current_tile = movement.GetCurrentTile();
                TileBehavior check_tile = WorldMap.instance.GetTopTile(current_tile.IsoCoordinates + offset);
                if (check_tile != null && check_tile.Fire.state == FireBehaviour.burnState.burning) {
                    return true;
                }
            }
        }
        return false;
    }
    private TileBehavior GetHighestDangerFire()
    {
        TileBehavior dangerousFire = null;
        float highestDanger = 0;
        for (int i = -FireSenseDistanceSquares; i < FireSenseDistanceSquares; i++) {
            for (int j = -FireSenseDistanceSquares; i < FireSenseDistanceSquares; i++) {
                Vector3Int offset = new Vector3Int(i, j, 0);
                TileBehavior current_tile = movement.GetCurrentTile();
                TileBehavior check_tile = WorldMap.instance.GetTopTile(current_tile.IsoCoordinates + offset);
                if (check_tile.Fire.state == FireBehaviour.burnState.burning && check_tile.Fire.dangerRating > highestDanger) {
                    dangerousFire = check_tile;
                    highestDanger = check_tile.Fire.dangerRating;
                }
            }
        }
        return dangerousFire;
    }

    private void idleUpdate()
    {
        if (CanSeeFire()) {
            enterState(VillagerState.ALERTED);
        }
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
        Debug.Log("Villager entered " + new_state.ToString());
        State = new_state;
    }

    private void on_enterPanicking()
    {
        throw new NotImplementedException();
    }

    private void on_enterPuttingOutFire()
    {
        if (CurrentFire) {
            CurrentTarget = CurrentFire;
            movement.GoToNeighborOf(CurrentFire);
        }
    }

    private void on_enterGettingWater()
    {
        List<TileBehavior> water_sources = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.WATER);
        if (water_sources.Count == 0) {
            enterState(VillagerState.PANICKING);
            return;
        }
        float closestWaterDistance = float.PositiveInfinity;
        TileBehavior closestWater = null;
        foreach (TileBehavior water in water_sources) {
            float waterDistance = (water.WorldCoordinates - transform.position).magnitude;
            if (waterDistance < closestWaterDistance) {
                closestWater = water;
                closestWaterDistance = waterDistance;
            }
        }
        CurrentTarget = closestWater;
        movement.GoToNeighborOf(closestWater);
    }

    private void on_enterAlerted()
    {
        CurrentFire = GetHighestDangerFire();
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
