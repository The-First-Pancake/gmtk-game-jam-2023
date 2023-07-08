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
    public List<TileBehavior> SeenFires;
    public float StateMachineTickRateSeconds = 1f;
    public int FireSenseDistanceSquares = 5;
    public float RoamSpeed = 25f;
    public float FireSpeed = 50f;

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<VillagerMovement>();
        InvokeRepeating("TickStateMachine", 0.1f, StateMachineTickRateSeconds);
    }

    // Update is called once per frame
    void Update()
    {
        TileBehavior CurrentTile = movement.GetCurrentTile();
        if (CurrentTile != null && CurrentTile.Fire.state == FireBehaviour.burnState.burning) {
            Destroy(gameObject);
        }
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
                if (LookForFires()) {
                    enterState(VillagerState.ALERTED);
                    return;
                } else {
                    enterState(VillagerState.IDLE);
                    return;
                }
            }
        }

        // Check if we're still heading to the highest danger fire we can see
        LookForFires();
        TileBehavior dangerFire = GetHighestDangerFire();
        if (dangerFire) {
            if (CurrentTarget != dangerFire || CurrentTarget == null) {
                CurrentTarget = dangerFire;
                movement.GoToNeighborOf(CurrentTarget);
            }
        } else {
            enterState(VillagerState.IDLE);
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
        } else if (LookForFires()) {
            enterState(VillagerState.ALERTED);
        }
    }

    private bool LookForFires() {
        for (int i = -FireSenseDistanceSquares; i < FireSenseDistanceSquares; i++) {
            for (int j = -FireSenseDistanceSquares; j < FireSenseDistanceSquares; j++) {
                Vector3Int offset = new Vector3Int(i, j, 0);
                TileBehavior current_tile = movement.GetCurrentTile();
                if (current_tile) {
                    TileBehavior check_tile = WorldMap.instance.GetTopTile(current_tile.IsoCoordinates + offset);
                    if (check_tile != null && check_tile.Fire.state == FireBehaviour.burnState.burning) {
                        if (!SeenFires.Contains(check_tile)) {
                            SeenFires.Add(check_tile);
                        }
                    }
                }
            }
        }
        // clean list
        SeenFires.RemoveAll(item => item == null);
        SeenFires.RemoveAll(item => item.Fire.state != FireBehaviour.burnState.burning);
        return (SeenFires.Count > 0);
    }
    private TileBehavior GetHighestDangerFire()
    {
        TileBehavior dangerousFire = null;
        float highestDanger = 0;
        foreach (TileBehavior fire in SeenFires)
        {
            if (fire != null && fire.Fire.state == FireBehaviour.burnState.burning && fire.Fire.dangerRating > highestDanger) {
                dangerousFire = fire;
                highestDanger = fire.Fire.dangerRating;
            }
        }
        return dangerousFire;
    }

    private void idleUpdate()
    {
        if (LookForFires()) {
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
        State = new_state;
    }

    private void on_enterPanicking()
    {
        throw new NotImplementedException();
    }

    private void on_enterPuttingOutFire()
    {
        TileBehavior fire = GetHighestDangerFire();
        if (fire) {
            CurrentTarget = fire;
            movement.GoToNeighborOf(fire);
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
        movement.BaseSpeed = FireSpeed;
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
        movement.BaseSpeed = RoamSpeed;
        movement.GoToTile(CurrentTarget);
    }

    private void on_enterIdle()
    {   
        // Nothing to do here... for now
        return;
    }
}
