using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class Agent_Mover : MonoBehaviour
{
    public PathFinder pathfinder;
    public float moveSpeed = 2f;

    // We need a coroutine to move the agent over time while allowing other code to run
    private Coroutine moveRoutine;

    private InputAction followAction;

    private void LateUpdate()
    {
        StartFollowPath();
    }

    private void OnEnable()
    {
        followAction = new InputAction(
            name: "FollowPath",
            type: InputActionType.Button,
            binding: "<Keyboard>/r"
        );

        followAction.performed += OnFollowPerformed;
        followAction.Enable();
    }

    private void OnDisable()
    {
        if (followAction != null)
        {
            followAction.performed -= OnFollowPerformed;
            followAction.Disable();
        }
    }

    private void OnFollowPerformed(InputAction.CallbackContext ctx)
    {
        StartFollowPath();
    }

    private void StartFollowPath()
    {
        if (pathfinder == null)
        {
            Debug.LogWarning("AgentMover: Pathfinder reference missing.");
            return;
        }

        // Run pathfinding (and update visuals)
        // We'll reuse the pathfinder's lastPath

        var grid = pathfinder.gridManager;
        if (grid == null)
        {
            Debug.LogWarning("AgentMover: GridManager reference missing.");
            return;
        }

        Node startNode = grid.GetNodeFromWorldPosition(transform.position);
        Node goalNode = grid.GetNodeFromWorldPosition(pathfinder.goalMarker.position);

        if (startNode == null || goalNode == null)
        {
            Debug.LogWarning("AgentMover: Invalid start or goal node.");
            return;
        }

        var openVisual = new HashSet<Node>();
        var closedVisual = new HashSet<Node>();
        List<Node> path = pathfinder.FindPath(startNode, goalNode, openVisual, closedVisual);

        if (path == null || path.Count == 0)
        {
            Debug.Log("AgentMover: No path found.");
            return;
        }

        // Stop any previous movement
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(FollowPath(path));
    }

    private IEnumerator FollowPath(List<Node> path)
    {
        // Skip first node if it's the tile we're already on
        int startIndex = 0;
        Node first = path[0];
        Vector3 firstWorldPos = new Vector3(
            first.X * pathfinder.gridManager.cellSize,
            0f,
            first.Y * pathfinder.gridManager.cellSize
        );

        // Check if we're already at the first node's position
        if (Vector3.Distance(transform.position, firstWorldPos) < 0.1f && path.Count > 1)
        {
            startIndex = 1;
        }

        for (int i = 0; i < path.Count; i++)
        {
            Node node = path[i];
            // Calculate target world position
            Vector3 targetPos = new Vector3(
                node.X * pathfinder.gridManager.cellSize,
                transform.position.y,
                node.Y * pathfinder.gridManager.cellSize
            );

            // Move towards target position
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null; // Wait for next frame
            }
        }

        moveRoutine = null; // Movement complete

    }

}


