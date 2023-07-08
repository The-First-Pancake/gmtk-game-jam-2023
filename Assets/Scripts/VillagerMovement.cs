using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerMovement : MonoBehaviour
{
    public float BaseSpeed = 1f;
    public List<TileBehavior> CurrentPath;
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

    public void GoToTile(TileBehavior target) {
        TileBehavior start = GetCurrentTile();
        CurrentPath = PathFind(start, target);
    }

    public TileBehavior GetCurrentTile() {
        return WorldMap.instance.GetTopTileFromWorldPoint(transform.position);
    }

    public bool IsDoneMove() {
        return (CurrentPath.Count == 0);
    }

    void FollowPath() {
        if (CurrentPath.Count > 0) {
            Vector3 target = CurrentPath[0].WorldCoordinates;
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
            Vector3 velocity = Time.deltaTime * BaseSpeed * (1 - current_tile.MovementModifier) * direction_of_travel;
            rb2d.velocity = velocity;
        }
    }

    void OnDrawGizmos() {
        // Draw path for debugging
        Gizmos.color = Color.blue;
        for (int i = 0; i < CurrentPath.Count - 1; i++) {
            Gizmos.DrawLine(CurrentPath[i].WorldCoordinates, CurrentPath[i+1].WorldCoordinates);
        }
    }

    public List<TileBehavior> PathFind(TileBehavior start, TileBehavior end) {
        List<PathFindingNode> open_list = new List<PathFindingNode>();
        List<PathFindingNode> closed_list = new List<PathFindingNode>();

        PathFindingNode startNode = new PathFindingNode(start);
        startNode.gCost = 0;
        startNode.hCost = Vector3Int.Distance(startNode.tile.IsoCoordinates, end.IsoCoordinates);
        open_list.Add(startNode);

        while (open_list.Count > 0) {
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
                Debug.Log(newMovementCostToNeighbor);
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
        return (CheckContainsTile(tile, closed_list) == null);
    }

    private List<TileBehavior> RetracePath(PathFindingNode startNode, PathFindingNode targetNode)
    {
        List<TileBehavior> path = new List<TileBehavior>();
        PathFindingNode currentNode = targetNode;
        while (currentNode != startNode) {
            Debug.Log(currentNode);
            path.Add(currentNode.tile);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }
}
