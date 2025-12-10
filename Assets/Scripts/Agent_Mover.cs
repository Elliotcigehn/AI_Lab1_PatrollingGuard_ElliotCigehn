using System.Collections.Generic;
using UnityEngine;

public class Agent_Mover : MonoBehaviour
{
    public PathFinder pathfinder;
    public float moveSpeed = 3f;
    private List<Node> currentPath;
    private int currentIndex = 0;
    public void FollowPath(List<Node> path)
    {
        currentPath = path;
        currentIndex = 0;
    }
    private void Update()
    {
        if (currentPath == null || currentPath.Count == 0) return;
        Node targetNode = currentPath[currentIndex];
        //Vector3 targetPos = NodeToWorldPosition(targetNode);
        // Move towards targetPos...
        // When close enough:
        // currentIndex++;
        // if currentIndex >= currentPath.Count, we’re done.
    }
    /*private Vector3 NodeToWorldPosition(Node node)
    {
       return new Vector3(node.x * tileSize, transform.position.y,
        node.y * tileSize);
    }*/

}
