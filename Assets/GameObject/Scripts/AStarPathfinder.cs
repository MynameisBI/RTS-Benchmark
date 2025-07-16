using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinder
{
    public class Node
    {
        public Vector2Int Position;
        public int GCost; // Distance from start
        public int HCost; // Heuristic to target
        public int FCost => GCost + HCost;

        public Node Parent;
        public bool Walkable;

        public Node(Vector2Int pos, bool walkable)
        {
            Position = pos;
            Walkable = walkable;
        }
    }

    private Node[,] grid;
    private int width, height;

    public AStarPathfinder(bool[,] walkableMap)
    {
        width = walkableMap.GetLength(0);
        height = walkableMap.GetLength(1);
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new Node(new Vector2Int(x, y), walkableMap[x, y]);
    }

    public AStarPathfinder(bool[] walkableMap, int width, int height)
    {
        this.width = width;
        this.height = height;
        grid = new Node[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new Node(new Vector2Int(x, y), walkableMap[x + y * width]);
    }

    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos)
    {
        var openSet = new List<Node>();
        var closedSet = new HashSet<Node>();

        Node startNode = grid[startPos.x, startPos.y];
        Node targetNode = grid[targetPos.x, targetPos.y];

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < current.FCost ||
                    (openSet[i].FCost == current.FCost && openSet[i].HCost < current.HCost))
                {
                    current = openSet[i];
                }
            }

            openSet.Remove(current);
            closedSet.Add(current);

            if (current == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbors(current))
            {
                if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    continue;

                int newCost = current.GCost + GetDistance(current, neighbor);
                if (newCost < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newCost;
                    neighbor.HCost = GetDistance(neighbor, targetNode);
                    neighbor.Parent = current;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    private List<Node> GetNeighbors(Node node)
    {
        var neighbors = new List<Node>();
        var dirs = new Vector2Int[] {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        foreach (var dir in dirs)
        {
            Vector2Int neighborPos = node.Position + dir;
            if (neighborPos.x >= 0 && neighborPos.x < width &&
                neighborPos.y >= 0 && neighborPos.y < height)
            {
                neighbors.Add(grid[neighborPos.x, neighborPos.y]);
            }
        }

        return neighbors;
    }

    private int GetDistance(Node a, Node b)
    {
        return Mathf.Abs(a.Position.x - b.Position.x) + Mathf.Abs(a.Position.y - b.Position.y);
    }

    private List<Vector2Int> RetracePath(Node start, Node end)
    {
        var path = new List<Vector2Int>();
        Node current = end;

        while (current != start)
        {
            path.Add(current.Position);
            current = current.Parent;
        }

        path.Reverse();
        return path;
    }
}
