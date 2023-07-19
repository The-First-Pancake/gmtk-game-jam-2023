using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillagerMovement : MonoBehaviour
{
    public float BaseSpeed = 1f;
    public List<Vector3> CurrentPath;
    public class PathFindingNode {
        public float gCost;
        public float hCost;
        public PathFindingNode parent;
        public TileBehavior tile;
        public float fCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        public PathFindingNode(TileBehavior tile_behavior) {
            gCost = float.PositiveInfinity;
            hCost = float.PositiveInfinity;
            tile = tile_behavior;
        }
    }

    private Rigidbody2D rb2d;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        FollowPath();
    }

    public bool GoToTile(TileBehavior target) {
        TileBehavior start = GetCurrentTile();
        List<Vector3> path = PathFind(start, target);
        if (path != null) {
            CurrentPath = path;
            return true;
        }
        Debug.Log("No path found.");
        return false;
    }

    public bool GoToNeighborOf(TileBehavior target) {
        List<TileBehavior> neighbors = target.GetNeighbors();
        // Sort by closest to current location
        neighbors = neighbors.OrderBy(x => Vector3Int.Distance(GetCurrentTile().IsoCoordinates, x.IsoCoordinates)).ToList();
        foreach (TileBehavior neighbor in neighbors) {
            if (neighbor.CanPath != TileBehavior.PathAble.BLOCKS_MOVEMENT &&
                    neighbor.Fire.state != FireBehaviour.burnState.burning) {
                if(GoToTile(neighbor)) {
                    // Append a final leg to the journey to travel to the edge of the target tile.
                    // Animations like bucket don't look right from the center of the tile
                    Vector3 inbetween = (neighbor.WorldCoordinates + target.WorldCoordinates) / 2;
                    CurrentPath.Add(inbetween);
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    public TileBehavior GetCurrentTile() {
        return WorldMap.instance.GetTopTileFromWorldPoint(transform.position);
    }

    public bool IsDoneMove() {
        return (CurrentPath.Count == 0);
    }

    public void CancelMove() {
        CurrentPath.Clear();
    }

    void FollowPath() {
        if (CurrentPath.Count > 0) {
            Vector3 target = CurrentPath[0];
            Vector3 direction_of_travel = target - transform.position;
            if (direction_of_travel.magnitude < 0.1) {
                CurrentPath.RemoveAt(0);
                if (CurrentPath.Count <= 0) {
                    rb2d.velocity = Vector3.zero;
                }
                return;
            }
            direction_of_travel /= direction_of_travel.magnitude;
            TileBehavior current_tile = WorldMap.instance.GetTopTileFromWorldPoint(transform.position);
            Vector3 velocity = Vector3.zero;
            if (current_tile) {
                velocity = BaseSpeed * (1 - current_tile.MovementModifier) * direction_of_travel;
            } else {
                velocity = BaseSpeed * direction_of_travel;
            }   
            rb2d.velocity = velocity;
        } else {
            rb2d.velocity = Vector3.zero;
        }
    }

    void OnDrawGizmos() {
        // Draw path for debugging
        Gizmos.color = Color.blue;
        for (int i = 0; i < CurrentPath.Count - 1; i++) {
            Gizmos.DrawLine(CurrentPath[i], CurrentPath[i+1]);
        }
    }

    public List<Vector3> PathFind(TileBehavior start, TileBehavior end) {
        int iterations = 0;
        List<PathFindingNode> open_list = new List<PathFindingNode>();
        List<PathFindingNode> closed_list = new List<PathFindingNode>();

        PathFindingNode startNode = new PathFindingNode(start);
        startNode.gCost = 0;
        startNode.hCost = Vector3Int.Distance(startNode.tile.IsoCoordinates, end.IsoCoordinates);
        open_list.Add(startNode);

        while (open_list.Count > 0) {
            if (iterations++ > 500) {
                return null;
            }
            PathFindingNode currentNode = open_list[0];
            foreach (PathFindingNode node in open_list) {
                if (node.fCost < currentNode.fCost || node.fCost == currentNode.fCost && node.hCost < currentNode.hCost) {
                    currentNode = node;
                }
            }

            closed_list.Add(currentNode);
            open_list.Remove(currentNode);

            if (currentNode.tile == end) {
                return RetracePath(startNode, currentNode);
            }

            foreach (TileBehavior neighbor_tile in currentNode.tile.GetNeighbors()) {
                if (!CheckCanPath(neighbor_tile, closed_list)) continue;
                PathFindingNode neighbor_node = CheckContainsTile(neighbor_tile, open_list);
                if (neighbor_node == null)
                    neighbor_node = new PathFindingNode(neighbor_tile);

                float newMovementCostToNeighbor = currentNode.gCost + Vector3Int.Distance(currentNode.tile.IsoCoordinates, neighbor_node.tile.IsoCoordinates);
                newMovementCostToNeighbor += neighbor_node.tile.MovementModifier; // Add our own arbitrary modifier for difficult/easy places to move into
                if (newMovementCostToNeighbor < neighbor_node.gCost) {
                    neighbor_node.gCost = newMovementCostToNeighbor;
                    neighbor_node.hCost = Vector3Int.Distance(neighbor_node.tile.IsoCoordinates, end.IsoCoordinates);
                    neighbor_node.parent = currentNode;
                }

                if (!open_list.Contains(neighbor_node))
                    open_list.Add(neighbor_node);
            }
        }
        return null;
    }

    private PathFindingNode CheckContainsTile(TileBehavior tile, List<PathFindingNode> list) {
        foreach (PathFindingNode node in list) {
            if (node.tile == tile) {
                return node;
            }
        }
        return null;
    }

    private bool CheckCanPath(TileBehavior tile, List<PathFindingNode> closed_list) {
        if (tile.CanPath == TileBehavior.PathAble.BLOCKS_MOVEMENT) {
            return false;
        }
        if (tile.Fire.state == FireBehaviour.burnState.burning) {
            return false;
        }
        return (CheckContainsTile(tile, closed_list) == null);
    }

    private List<Vector3> RetracePath(PathFindingNode startNode, PathFindingNode targetNode)
    {
        List<Vector3> path = new List<Vector3>();
        PathFindingNode currentNode = targetNode;
        while (currentNode != startNode) {
            path.Add(currentNode.tile.WorldCoordinates);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }
}
