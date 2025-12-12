using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PathFinder : MonoBehaviour
{
    public GridManager gridManager;

    [Header("Start & Goal Nodes")]
    public Transform startMarker;
    public Transform goalMarker;
    public Transform chaseMarker;

    [Header("Materials")]
    public Material PathMaterial;
    public Material openMaterial;
    public Material closedMaterial;
    public Material GoalMaterial;

    private List<Node> lastPath;

    private InputAction pathfindAction;
    private InputAction GoalAction;
    private InputAction ChaseAction;

    private void RunPathFinding()
    {
        if (gridManager == null || startMarker == null || goalMarker == null)
        {
            Debug.LogError("PathFinder: Missing references in the inspector.");
            return;
        }

        Node startNode = gridManager.GetNodeFromWorldPosition(startMarker.position);
        Node goalNode = gridManager.GetNodeFromWorldPosition(goalMarker.position);

        if (startNode == null || goalNode == null)
        {
            Debug.LogError("PathFinder: Start or Goal node is null.");
            return;
        }

        ResetGridVisuals();


        HashSet<Node> openVisual = new HashSet<Node>();
        HashSet<Node> closedVisual = new HashSet<Node>();

        lastPath = FindPath(startNode, goalNode, openVisual, closedVisual);

        foreach (var node in openVisual)
        {
            if (node.Walkable)
            {
                SetTileMaterialSafe(node, openMaterial);
            }
        }

        foreach (var node in closedVisual)
        {
            if (node.Walkable)
            {
                SetTileMaterialSafe(node, closedMaterial);
            }
        }

        if (lastPath != null)
        {
            foreach (var node in lastPath)
            {
                SetTileMaterialSafe(node, PathMaterial);
            }
        }
        else
        {
            Debug.Log("PathFinder: No path found.");
        }

        SetTileMaterialSafe(startNode, PathMaterial);
        SetTileMaterialSafe(goalNode, PathMaterial);
    }

    private void ResetGridVisuals()
    {
        for (int x = 0; x < gridManager.width; x++)
        {
            for (int y = 0; y < gridManager.height; y++)
            {
                Node node = gridManager.GetNode(x, y);
                if (node.Walkable)
                {
                    SetTileMaterialSafe(node, gridManager.walkableMaterial);
                }
                else
                {
                    SetTileMaterialSafe(node, gridManager.wallMaterial);
                }
            }
        }
    }

    private void SetTileMaterialSafe(Node node, Material material)
    {
        var renderer = node.Tile.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }
    }

    public List<Node> FindPath(Node startNode, Node goalNode, HashSet<Node> openVisual, HashSet<Node> closedVisual)
    {
        gridManager.ResetAllNodes();

        List<Node> openSet = new List<Node>(); // Nodes to be evaluated
        HashSet<Node> closedSet = new HashSet<Node>(); // Nodes already evaluated

        startNode.GCost = 0f; // Cost so far to reach start node is zero initially
        startNode.HCost = Heuristic(startNode, goalNode); // First guess, from start to goal
        openSet.Add(startNode); // Add start node to open set, we will evaluate it first
        openVisual?.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get node in open set with lowest F cost
            Node current = GetLowestFCostNode(openSet);

            if (current == goalNode)
            {
                // Found our goal node
                // When we reach here, we can reconstruct the path
                // by following parent nodes from goal to start
                return ReconstructPath(startNode, goalNode);
            }

            openSet.Remove(current);
            closedSet.Add(current); // Mark current node as evaluated
            closedVisual?.Add(current);

            // Explore neighbours to see if we can find a better path
            foreach (Node neighbour in gridManager.GetNeighbours(current))
            {
                if (neighbour == null || !neighbour.Walkable)
                    // Skip non-walkable or null neighbours
                    continue;
                if (closedSet.Contains(neighbour))
                    // Already evaluated
                    continue;

                // cost(current, neighbour) = 1. Unweighted grid.
                // Tentative = cost so far to reach neighbour
                float tentativeG = current.GCost + 1f;

                if (tentativeG < neighbour.GCost)
                {
                    // Found a better path to neighbour
                    neighbour.Parent = current;
                    neighbour.GCost = tentativeG; // Update cost to reach neighbour
                    neighbour.HCost = Heuristic(neighbour, goalNode);

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                        openVisual?.Add(neighbour);
                    }
                }
            }
        }

        // No path found
        return null;
    }

    private Node GetLowestFCostNode(List<Node> openSet)
    {
        Node best = openSet[0]; // Assume first node is best initially
        for (int i = 1; i < openSet.Count; i++)
        {
            // For each node, check if its F cost is lower than the best found so far
            Node candidate = openSet[i];
            if (candidate.FCost < best.FCost ||
                Mathf.Approximately(candidate.FCost, best.FCost) && candidate.HCost < best.HCost)
            {
                best = candidate;
            }
        }
        return best;
    }
    private float Heuristic(Node a, Node b)
    {
        //return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        int dx = Mathf.Abs(a.X - b.X);
        int dy = Mathf.Abs(a.Y - b.Y);
        return dx + dy;
    }
    private List<Node> ReconstructPath(Node startNode, Node goalNode)
    {
        List<Node> path = new List<Node>();
        Node current = goalNode;

        while (current != null)
        {
            path.Add(current);
            if (current == startNode)
                break;
            current = current.Parent;
        }

        path.Reverse(); // Reverse to get path from start to goal
        return path;
    }

    private void OnEnable()
    {
        pathfindAction = new InputAction(
            name: "FindPath",
            type: InputActionType.Button,
            binding: "<Keyboard>/f"
        );
        pathfindAction.performed += OnPathFinderPerformed;
        pathfindAction.Enable();

        GoalAction = new InputAction(
            name: "SetGoal",
            type: InputActionType.Button,
            binding: "<Keyboard>/e"
        );
        GoalAction.performed -= ctx => SetGoalNode(); // Unsubscribe first to avoid multiple subscriptions
        GoalAction.performed += ctx => SetGoalNode(); // Subscribe to event
        GoalAction.Enable();

        ChaseAction = new InputAction(
            name: "ChaseGoal",
            type: InputActionType.Button,
            binding: "<Keyboard>/q"
        );
        ChaseAction.performed -= ctx => SetChaseNode(); // Unsubscribe first to avoid multiple subscriptions
        ChaseAction.performed += ctx => SetChaseNode(); // Subscribe to event
        ChaseAction.Enable();
    }

    private void OnDisable()
    {
        if (pathfindAction != null)
        {
            pathfindAction.performed -= OnPathFinderPerformed;
            pathfindAction.Disable();
        }
        if (GoalAction != null)
        {
            GoalAction.performed -= ctx => SetGoalNode();
            GoalAction.Disable();
        }
    }

    private void OnPathFinderPerformed(InputAction.CallbackContext context)
    {
        RunPathFinding();
    }

    private void SetGoalNode()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Vector3 mousePosition = hitInfo.collider.gameObject.transform.position;
            gridManager.SetTileMaterial(gridManager.GetNodeFromWorldPosition(mousePosition), GoalMaterial);
            if (goalMarker != null)
            {
                gridManager.SetTileMaterial(gridManager.GetNodeFromWorldPosition(goalMarker.position), gridManager.walkableMaterial);
                goalMarker = null;
            }
            goalMarker = hitInfo.collider.transform;
            return;
        }

    }

    private void SetChaseNode()
    {
        goalMarker = chaseMarker;
    }

}
