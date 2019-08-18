using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#region PriorityQueue

/// <summary>
/// Simple Priority Queue class that sorts on dequeue
/// </summary>
public class PriorityQueue<T>
{
    private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

    public int Count
    {
        get { return elements.Count; }
    }

    public void Enqueue(T item, float priority)
    {
        elements.Add(new KeyValuePair<T, float>(item, priority));
    }

    // Returns the Vector3Int that has the lowest priority
    public T Dequeue()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i].Value < elements[bestIndex].Value)
            {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Key;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}

#endregion

public static class Astar
{
    static GridPoint start;
    static GridPoint goal;


    /// <summary>
    /// Resets the variables linked to A* in the graph
    /// </summary>
    static void CleanGraph()
    {
        foreach (GridPoint point in RegularGrid.graph.nodes)
        {
            point.costSoFar = 0;
            point.prev = null;
        }
    }

    /// <summary>
    /// Determines the start and goal nodes, performs A* and create the path
    /// </summary>
    public static List<Vector3> CreateAndMakePath(Vector3 startNode, Vector3 end)
    {
        CleanGraph();

        FindClosestPoints(startNode, end);
        
        FindPath();

        GridPoint current = goal;

        List<Vector3> path = new List<Vector3>();
        while (!current.Equals(start))
        {
            path.Insert(0, current.pos);
            current = current.prev;
        }
        path.Add(goal.pos);
        path = smoothPath(path);
        start.prev = null;

        return path;
    }

    /// <summary>
    /// Smooths out the path for the Regular Grid
    /// </summary>
    static List<Vector3>   smoothPath(List<Vector3> path)
    {
        List<Vector3> newPath = new List<Vector3>();

        newPath.Add(path[0]);
        for (int i = 2; i < path.Count - 1; i++)
        {
            Vector3 pos = newPath[newPath.Count - 1];
            Vector3 direction = path[i] - newPath[newPath.Count - 1];
            float distance = Vector3.Distance(path[i], newPath[newPath.Count - 1]);

            if (Physics.SphereCast(pos, 0.2f, direction, out RaycastHit hit, distance) || Physics.Raycast(pos, direction, distance))
            {
                newPath.Add(path[i - 1]);
            }
        }
        newPath.Add(path[path.Count - 1]);
        return newPath;
    }

    /// <summary>
    /// Returns the distance between current node and goal node
    /// </summary>
    static float HeuristicEuclidian(GridPoint node)
    {
        return Vector3.Distance(node.pos, goal.pos);
    }

    /// <summary>
    /// Performs A*
    /// </summary>
    static void FindPath()
    {
        PriorityQueue<GridPoint> list = new PriorityQueue<GridPoint>(); //open List

        list.Enqueue(start, 0f);

        while (list.Count > 0)  //while there are still nodes to process
        {
            GridPoint current = list.Dequeue();

            if (goal.costSoFar != 0 && current.costSoFar + HeuristicEuclidian(current) > goal.costSoFar) //if the estimated total-cost is bigger than the costSoFar of the goalNode A* is finished
                break;

            if (current.Equals(goal))
            {
                continue;
            }

            foreach (Connection conn in current.connections)
            {
                float newCost = current.costSoFar + conn.distance;
                if (conn.neighbour.costSoFar == 0 || newCost < conn.neighbour.costSoFar) //if it has not been processed or has been found again in a faster path process node
                {
                    conn.neighbour.costSoFar = newCost;
                    conn.neighbour.prev = current;

                    float priority = newCost + HeuristicEuclidian(conn.neighbour);
                    list.Enqueue(conn.neighbour, priority);
                }
            }
        }
    }

    /// <summary>
    /// Finds Start / End node from position
    /// </summary>
    private static void FindClosestPoints(Vector3 start, Vector3 end)
    {
        float minDistStart = Mathf.Infinity;
        float minDistEnd = Mathf.Infinity;

        GridPoint tmpStart = null;
        GridPoint tmpEnd = null;

        foreach (GridPoint p in RegularGrid.graph.nodes)
        {
            float distStart = Vector3.Distance(start, p.pos);
            float distEnd = Vector3.Distance(end, p.pos);

            if (distStart < minDistStart)
            {
                minDistStart = distStart;
                tmpStart = p;
            }
            if (distEnd < minDistEnd)
            {
                minDistEnd = distEnd;
                tmpEnd = p;
            }
        }
        Astar.start = tmpStart;
        Astar.goal = tmpEnd;
    }
}
