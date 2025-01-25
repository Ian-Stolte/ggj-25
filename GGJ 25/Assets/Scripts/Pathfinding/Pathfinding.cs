using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    [HideInInspector] public Grid grid;

    public IEnumerator FindPath(Vector3 startPos, Vector3 endPos, bool bigGrid, Action<Vector3[], bool> callback, bool waitAFrame)
    {
        if (bigGrid)
            grid = transform.GetChild(0).GetComponent<Grid>();
        else
            grid = GetComponent<Grid>();

        Vector3[] waypoints = new Vector3[0];
        bool success = false;

        Node start = grid.NodeFromWorldPoint(startPos);
        Node end = FindNearestWalkable(grid.NodeFromWorldPoint(endPos));
        if (end.walkable)
        {
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 0; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        currentNode = openSet[i];
                    }
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == end)
                {
                    RetracePath(start, end);
                    if (currentNode != start)
                        success = true;
                    break;
                }

                foreach (Node neighbor in grid.GetNeighbors(currentNode))
                {
                    if (!neighbor.walkable || closedSet.Contains(neighbor))
                        continue;

                    int newDist = currentNode.gCost + GetDist(currentNode, neighbor);
                    if (newDist < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newDist;
                        neighbor.hCost = GetDist(neighbor, end);
                        neighbor.parent = currentNode;
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }
        }
        if (success)
        {
            waypoints = RetracePath(start, end);
        }
        if (waitAFrame)
        {
            yield return null;
            callback(waypoints, success);
        }
        else
        {
            callback(waypoints, success);
            yield return null;
        }
    }

    private Node FindNearestWalkable(Node node)
    {
        if (node.walkable)
            return node;
        
        Node currentClosest = grid.grid[0, 0];
        foreach (Node n in grid.grid)
        {
            if (n.walkable && Vector2.Distance(node.position, n.position) < Vector2.Distance(node.position, currentClosest.position))
            {
                currentClosest = n;
            }
        }
        return currentClosest;

        //draw a line from end --> start
        /*Vector3 endToStartPos = end.position;
        while (!end.walkable && Vector2.Distance(endToStartPos, start.position) > 0.1f)
        {
            Node nodeToTry = GetComponent<Grid>().NodeFromWorldPoint(endToStartPos);
            if (nodeToTry.walkable)
                end = nodeToTry;
            else
                endToStartPos = Vector3.MoveTowards(endToStartPos, start.position, 0.1f);
        }*/
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        
        Node currentNode = endNode;
        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    private Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 oldDir = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 newDir = new Vector2(path[i-1].gridX - path[i].gridX, path[i-1].gridY - path[i].gridY);
            if (newDir != oldDir)
            {
                waypoints.Add(path[i-1].position);
            }
            oldDir = newDir;
        }
        //to display full path
        /*foreach (Node n in path)
        {
            waypoints.Add(n.position);
        }*/
        return waypoints.ToArray();
    }

    private int GetDist(Node a, Node b)
    {
        int xDist = Mathf.Abs(a.gridX - b.gridX);
        int yDist = Mathf.Abs(a.gridY - b.gridY);

        if (xDist < yDist)
            return 14*xDist + 10*(yDist-xDist);
        else
            return 14*yDist + 10*(xDist-yDist);
    }
}
