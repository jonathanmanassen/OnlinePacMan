using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularGrid : MonoBehaviour
{
    public ColorVue colorVue;

    readonly float step = 0.355f;
    public static GridPoint[,] grid;
    public static Graph graph;

    public static RegularGrid instance;

    public GridPoint GetClosestGridPointLocation(Vector3 position)
    {
        float min = Mathf.Infinity;
        GridPoint tmp = null;

        foreach (GridPoint g in graph.nodes)
        {
            if (Vector3.Distance(g.pos, position) < min)
            {
                min = Vector3.Distance(g.pos, position);
                tmp = g;
            }
        }
        if (min > step)
            return tmp;
        return null;
    }

    public void LoadPacDots()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foreach (GridPoint g in graph.nodes)
        {
            Vector3 distPlane = new Vector3(step / 2f, 0.5f, step / 2f);
            Collider[] colliders = Physics.OverlapBox(g.pos, distPlane, Quaternion.identity);

            bool found = false;

            foreach (Collider c in colliders)
                if (c.tag == "noPacDot")
                {
                    found = true;
                    break;
                }

            if (!found)
                Instantiate(Resources.Load("PacDot"), g.pos, Quaternion.identity, transform);
        }
    }

    private void Awake()
    {
        CreateGrid();
        graph = new Graph(grid);
        instance = this;
    }

    private void Update()
    {
        if (transform.childCount == 0)
        {
            LoadPacDots();
        }
    }

    /// <summary>
    /// Creates a grid by traversing the plane with a step and placing nodes along the way
    /// </summary>
    void CreateGrid()
    {
        int j = 0;
        grid = new GridPoint[(int)(10f / step + 0.5f), (int)(10.7f / step + 0.5f)]; //the size of the grid depends on the step (distance between nodes)
        Vector3 dist = new Vector3(step / 2f, 0.1f, step / 2f);
        Vector3 distPlane = new Vector3(step / 2f, 0.5f, step / 2f);
        for (float y = -5.2f + step / 2; y < 5.5f; y += step, j++)   // traverses the plane vertically
        {
            int i = 0;
            for (float x = -5f + step / 2; x < 5f; x += step, i++)  // traverses the plane horizontally
            {
                Vector3 pos = new Vector3(x, 0, y);

                Collider[] colliders = Physics.OverlapBox(pos, distPlane, Quaternion.identity);

                bool found = false;
                foreach (Collider c in colliders)
                    if (c.tag == "Wall")
                    {
                        found = true;
                        break;
                    }
                if (found)
                    grid[i, j] = new GridPoint(false, pos, 0);
                else
                    grid[i, j] = new GridPoint(true, pos, 0);
            }
        }
        CreateConnections();
    }

    /// <summary>
    /// Creates the connections between the nodes by adding the valid nodes around it
    /// </summary>
    private void CreateConnections()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
            for (int y = 0; y < grid.GetLength(1); y++)
                if (grid[x, y].valid)
                {
                    for (int i = x - 1; i <= x + 1; i++)
                    {
                        if (i < 0 || i >= grid.GetLength(0))
                            continue;
                        for (int j = y - 1; j <= y + 1; j++)
                        {
                            if (j < 0 || j >= grid.GetLength(1) || (i == x && j == y))
                                continue;

                            if (grid[i, j].valid)
                                grid[x, y].connections.Add(new Connection(Vector3.Distance(grid[x, y].pos, grid[i, j].pos), grid[i, j]));
                        }
                    }
                }
    }

    /// <summary>
    /// Draws the graph on the screen when in editor view and pressing on the RegularGrid object
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        CreateGrid();
        if (graph == null || graph.nodes == null)
            graph = new Graph(grid);
        Vector3 size = new Vector3(step * 0.75f, 0.1f, step * 0.75f);

        foreach (GridPoint point in graph.nodes)
        {
            if (point.valid == true)
            {
                if (colorVue == ColorVue.CONNECTIONS)
                    Gizmos.color = Misc.GetColorWithVar(point.connections.Count);
                else if (colorVue == ColorVue.NONE)
                    Gizmos.color = Color.white;

                Gizmos.DrawCube(point.pos, size);
            }
        }
    }
}
