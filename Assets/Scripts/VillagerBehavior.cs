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
        SPLATSHING_WATER,
        PANICKING,
        BURNING,
        DYING,
    }
    public VillagerState State = VillagerState.IDLE;
    private VillagerMovement movement;
    public TileBehavior CurrentTarget;
    private Animator anim;
    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;
    public Sprite deadSprite;
    public GameObject onFirePrefab;
    private GameObject fireObject;
    public GameObject smolderingPrefab;
    public List<TileBehavior> SeenFires;
    public List<TileBehavior> UnreachableFires;
    public float StateMachineTickRateSeconds = 1f;
    public int FireSenseDistanceSquares = 5;
    public float RoamSpeed = 25f;
    public float FireSpeed = 50f;
    private bool wait_finished = false;
    public SpriteRenderer alert;
    // Start is called before the first frame update
    void Start()
    {
        alert.enabled = false;
        movement = GetComponent<VillagerMovement>();
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        InvokeRepeating("TickStateMachine", 0.1f, StateMachineTickRateSeconds);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimator();
        TileBehavior CurrentTile = movement.GetCurrentTile();
        if (CurrentTile != null && CurrentTile.Fire.state == FireBehaviour.burnState.burning) {
            if (State != VillagerState.BURNING && State != VillagerState.DYING) {
                enterState(VillagerState.BURNING);
                return;
            }
        }
    }

    private void UpdateAnimator() {
        Vector3 velocity = rb2d.velocity;
        switch (State) {
            case VillagerState.IDLE:
            case VillagerState.ROAMING:
            case VillagerState.ALERTED:
            case VillagerState.GETTING_WATER:
            case VillagerState.PUTTING_OUT_FIRE:
                float up_angle = Vector3.Angle(velocity, Vector3.up);
                float left_angle = Vector3.Angle(velocity, Vector3.left);
                float right_angle = Vector3.Angle(velocity, Vector3.right);
                float down_angle = Vector3.Angle(velocity, Vector3.down);
                if (up_angle <= left_angle && up_angle <= right_angle && up_angle <= down_angle) {
                    anim.SetTrigger("TurnAway");
                    return;
                } else if (left_angle <= up_angle && left_angle <= right_angle && left_angle <= down_angle) {
                    spriteRenderer.flipX = false;
                    anim.SetTrigger("TurnSide");
                    return;
                } else if (right_angle <= up_angle && right_angle <= left_angle && right_angle <= down_angle) {
                    spriteRenderer.flipX = true;
                    anim.SetTrigger("TurnSide");
                } else {
                    anim.SetTrigger("TurnForward");
                }
                break;
            case VillagerState.PANICKING:
                anim.SetTrigger("EnterPanic");
                break;
            case VillagerState.SPLATSHING_WATER:
                velocity = rb2d.velocity;
                float left = Vector3.Angle(velocity, Vector3.left);
                float right = Vector3.Angle(velocity, Vector3.right);
                spriteRenderer.flipX = right <= left;
                anim.SetTrigger("PourWater");
                break;
            case VillagerState.BURNING:
                anim.SetTrigger("StartBurn");
                break;
            case VillagerState.DYING:
                anim.SetTrigger("EnterDie");
                break;
            default:
                throw new NotImplementedException();
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
            case VillagerState.SPLATSHING_WATER:
                splashingUpdate();
                break;
            case VillagerState.BURNING:
                burningUpdate();
                break;
            case VillagerState.DYING:
                dyingUpdate();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void burningUpdate()
    {
        if (movement.IsDoneMove()) {
            enterState(VillagerState.DYING);
            return;
        }
    }

    private void dyingUpdate()
    {
        if (wait_finished) {
            wait_finished = false;
            Destroy(fireObject);
            Instantiate(smolderingPrefab, transform);
            alert.enabled = false;
            anim.enabled = false;
            spriteRenderer.sprite = deadSprite;
        }
    }

    private void splashingUpdate()
    {
        if (wait_finished) {
            if (CurrentTarget != null) {
                CurrentTarget.Fire.extinguish();
            }
            wait_finished = false;
            if (LookForFires()) {
                enterState(VillagerState.ALERTED);
                return;
            } else {
                enterState(VillagerState.IDLE);
                return;
            }
        }
    }

    private void panickingUpdate()
    {
        if (movement.IsDoneMove()) {
            enterState(VillagerState.PANICKING);
            return;
        }
    }

    private void puttingOutFireUpdate()
    {
        // Check if we can put out any fires
        TileBehavior currentTile = movement.GetCurrentTile();
        if (currentTile == null) {
            return;
        }
        // Always try to put out adjacent fires
        foreach (TileBehavior neighbor in currentTile.GetNeighbors()) {
            if (neighbor.Fire.state == FireBehaviour.burnState.burning) {
                CurrentTarget = neighbor;
                enterState(VillagerState.SPLATSHING_WATER);
                return;
            }
        }

        // Check if we're still heading to the highest danger fire we can see
        LookForFires();
        TileBehavior dangerFire = GetHighestDangerFire();
        while (dangerFire) {
            if (CurrentTarget != dangerFire || CurrentTarget == null) {
                CurrentTarget = dangerFire;
                if (!movement.GoToNeighborOf(CurrentTarget)) {
                    SeenFires.Remove(dangerFire);
                    UnreachableFires.Add(dangerFire);
                } else {
                    return;
                }
            } else {
                return;
            }
        }
    }

    private void gettingWaterUpdate()
    {
        if (CurrentTarget == null) {
            enterState(VillagerState.PANICKING);
            return;
        }
        if (movement.IsDoneMove()) {
            enterState(VillagerState.PUTTING_OUT_FIRE);
            return;
        } 
    }

    private void alertedUpdate()
    {
        List<TileBehavior> water_sources = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.WATER);
        if (water_sources.Count == 0) {
            enterState(VillagerState.PANICKING);
            return;
        }
        enterState(VillagerState.GETTING_WATER);
        return;
    }

    private void roamingUpdate()
    {
        if (movement.IsDoneMove()) {
            enterState(VillagerState.IDLE);
            return;
        } else if (LookForFires()) {
            enterState(VillagerState.ALERTED);
            return;
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
                        if (!SeenFires.Contains(check_tile) && !UnreachableFires.Contains(check_tile)) {
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
            return;
        }
        enterState(VillagerState.ROAMING);
        return;
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
            case VillagerState.SPLATSHING_WATER:
                on_enterSplashing();
                break;
            case VillagerState.BURNING:
                on_enterBurning();
                break;
            case VillagerState.DYING:
                on_enterDying();
                break;
            default:
                throw new NotImplementedException();
        }
        State = new_state;
    }

    private void on_enterBurning()
    {
        movement.CancelMove();
        fireObject = Instantiate(onFirePrefab, transform);
        Vector3Int randomVector = new Vector3Int(UnityEngine.Random.Range(-2, 2), (UnityEngine.Random.Range(-2, 2)));
        TileBehavior tile = WorldMap.instance.GetTopTile(movement.GetCurrentTile().IsoCoordinates + randomVector);
        if (tile) {
            movement.BaseSpeed = FireSpeed;
            CurrentTarget = tile;
            movement.GoToNeighborOf(CurrentTarget);
        }
    }

    private void on_enterDying()
    {
        movement.CancelMove();
        StartCoroutine(Wait(3));
    }

    private void on_enterSplashing()
    {
        movement.CancelMove();
        StartCoroutine(Wait(3));
    }

    IEnumerator Wait (float seconds) {
        wait_finished = false;
        yield return new WaitForSeconds (seconds);
        wait_finished = true;
    }

    private void on_enterPanicking()
    {
        Vector3Int randomVector = new Vector3Int(UnityEngine.Random.Range(-2, 2), (UnityEngine.Random.Range(-2, 2)));
        TileBehavior tile = WorldMap.instance.GetTopTile(movement.GetCurrentTile().IsoCoordinates + randomVector);
        if (tile) {
            movement.BaseSpeed = FireSpeed;
            CurrentTarget = tile;
            movement.GoToNeighborOf(CurrentTarget);
        }
    }

    private void on_enterPuttingOutFire()
    {
        // Nothing to do here
    }

    private void on_enterGettingWater()
    {
        List<TileBehavior> water_sources = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.WATER);
        float closestWaterDistance = float.PositiveInfinity;
        TileBehavior closestWater = null;
        foreach (TileBehavior water in water_sources) {
            float waterDistance = (water.WorldCoordinates - transform.position).magnitude;
            if (waterDistance < closestWaterDistance) {
                closestWater = water;
                closestWaterDistance = waterDistance;
            }
        }
        if (movement.GoToNeighborOf(closestWater)) {
            CurrentTarget = closestWater;
        } else {
            CurrentTarget = null;
        }
    }

    private void on_enterAlerted()
    {
        alert.enabled = true;
        movement.BaseSpeed = FireSpeed;
    }

    private void on_enterRoaming()
    {
        List<TileBehavior> buildings = WorldMap.instance.GetAllTilesOfTargetType(TileBehavior.VillagerTargetType.BUILDING);
        if (buildings.Count == 0) {return;}
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
        alert.enabled = false;
    }
}
