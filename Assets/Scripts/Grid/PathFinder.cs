using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PathFinder : MonoBehaviour
{
    public GridManager gridManager;

    [Header("Start & Goal Nodes")]
    public Transform startMarker;
    public Transform goalMarker;

    [Header("Materials")]
    public Material PathMaterial;
    public Material openMaterial;
    public Material closedMaterial;

    private List<Node> lastPath;

    private InputAction findPathAction;

    private List<Node> nodes = new List<Node>();

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
            Debug.LogError("PathFinder: Start or Goal node is invalid.");
            return;
        }

        ResetGridVisuals();


        HashSet<Node> openSetVisual = new HashSet<Node>();
        HashSet<Node> closedSetVisual = new HashSet<Node>();

        lastPath = FindPath(startNode, goalNode, openSetVisual, closedSetVisual);

        foreach (var node in openSetVisual)
        {
            if (node.walkable)
            {
                SetTileMaterialSafe(node, openMaterial);
            }
        }

        foreach (var node in closedSetVisual)
        {
            if (node.walkable)
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
        return;
    }

    private void SetTileMaterialSafe(Node node, Material material)
    {
        var renderer = node.tile.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.material = material;
        }
    }

    public List<Node> FindPath(Node startNode, Node goalNode, HashSet<Node> openSetVisual, HashSet<Node> closedSetVisual)
    {
        var openSet = new SortedSet<Node>(Comparer<Node>.Create((a, b) =>
        {
            int compare = a.fCost.CompareTo(b.fCost);
            if (compare == 0)
                compare = a.hCost.CompareTo(b.hCost);
            return compare;
        }));
        var cameFrom = new Dictionary<Node, Node>();
        startNode.gCost = 0;
        startNode.hCost = Heuristic(startNode, goalNode);
        openSet.Add(startNode);
        openSetVisual.Add(startNode);
        while (openSet.Count > 0)
        {
            Node current = openSet.Min;
            if (current == goalNode)
            {
                return ReconstructPath(cameFrom, current);
            }
            openSet.Remove(current);
            openSetVisual.Remove(current);
            closedSetVisual.Add(current);
            foreach (var neighbor in gridManager.GetNeighbors(current))
            {
                if (neighbor == null || !neighbor.walkable || closedSetVisual.Contains(neighbor))
                    continue;
                float tentativeGCost = current.gCost + 1;
                if (tentativeGCost < neighbor.gCost)
                {
                    cameFrom[neighbor] = current;
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = Heuristic(neighbor, goalNode);
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                        openSetVisual.Add(neighbor);
                    }
                }
            }
        }
        return null; // No path found
    }
    private float Heuristic(Node a, Node b)
    {
        //return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx + dy;
    }
    private List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
    {
        List<Node> totalPath = new List<Node> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        foreach (var node in totalPath)
        {
            node.tile.GetComponent<Renderer>().material = PathMaterial;
        }
        return totalPath;
    }

    private void OnEnable()
    {
        findPathAction.Enable();
        findPathAction = InputSystem.actions["Player/Interact"];
    }

    private void OnDisable()
    {
        if (findPathAction != null)
        {
            findPathAction.performed -= ctx => RunPathFinding();
            findPathAction.Disable();
        }
    }

}
