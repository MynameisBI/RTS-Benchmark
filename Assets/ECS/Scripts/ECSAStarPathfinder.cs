using System.Diagnostics;
using System.Net.NetworkInformation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct Node
{
    public int2 position;
    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;
    public int2 parent;
    public bool walkable;
}

[BurstCompile]
public struct ECSAStarPathfinder
{
    public int2 start;
    public int2 goal;
    public int2 gridSize;

    [ReadOnly] public DynamicBuffer<OccupationCellBuffer> occupationCells; // Buffer to store the occupation cells

    public DynamicBuffer<UnitPathBuffer> pathBuffer; // Buffer to store the path

    [BurstCompile]
    public bool Execute()
    {
        NativeList<int2> openList = new NativeList<int2>(Allocator.Temp);
        NativeHashSet<int2> closedSet = new NativeHashSet<int2>(100, Allocator.Temp);
        NativeParallelHashMap<int2, Node> cameFrom = new NativeParallelHashMap<int2, Node>(100, Allocator.Temp);

        Node startNode = new Node
        {
            position = start,
            gCost = 0,
            hCost = Heuristic(start, goal),
            parent = start,
            walkable = true
        };
        cameFrom[start] = startNode;
        openList.Add(start);

        while (openList.Length > 0)
        {
            int bestIndex = 0;
            float bestF = float.MaxValue;

            // Find best node in open list
            for (int i = 0; i < openList.Length; i++)
            {
                var pos = openList[i];
                var node = cameFrom[pos];
                if (node.fCost < bestF)
                {
                    bestF = node.fCost;
                    bestIndex = i;
                }
            }

            int2 current = openList[bestIndex];
            openList.RemoveAtSwapBack(bestIndex);

            if (current.Equals(goal))
            {
                ReconstructPath(cameFrom, current);
                openList.Dispose();
                closedSet.Dispose();
                cameFrom.Dispose();
                return true;
            }

            closedSet.Add(current);

            NativeArray<int2> neighborOffsets = new NativeArray<int2>(4, Allocator.Temp);
            neighborOffsets[0] = new int2(1, 0); // Right
            neighborOffsets[1] = new int2(-1, 0); // Left
            neighborOffsets[2] = new int2(0, 1); // Up
            neighborOffsets[3] = new int2(0, -1); // Down
            for (int i = 0; i < 4; i++)
            {
                int2 neighbor = current + neighborOffsets[i];

                if (!IsInBounds(neighbor) || !IsWalkable(neighbor) || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = cameFrom[current].gCost + 1f;

                if (!cameFrom.TryGetValue(neighbor, out Node neighborNode) || tentativeG < neighborNode.gCost)
                {
                    neighborNode = new Node
                    {
                        position = neighbor,
                        gCost = tentativeG,
                        hCost = Heuristic(neighbor, goal),
                        parent = current,
                        walkable = true
                    };

                    cameFrom[neighbor] = neighborNode;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
            neighborOffsets.Dispose();
        }

        openList.Dispose();
        closedSet.Dispose();
        cameFrom.Dispose();

        return false;
    }

    void ReconstructPath(NativeParallelHashMap<int2, Node> cameFrom, int2 current)
    {
        NativeList<int2> tempPath = new NativeList<int2>(Allocator.Temp);
        while (!current.Equals(start))
        {
            tempPath.Add(current);
            current = cameFrom[current].parent;
        }
        tempPath.Add(start);

        // Reverse
        for (int i = tempPath.Length - 1; i >= 0; i--)
            pathBuffer.Add(new UnitPathBuffer
            {
                position = new int2(tempPath[i].x, tempPath[i].y)
            });
    }

    float Heuristic(int2 a, int2 b) => math.abs(a.x - b.x) + math.abs(a.y - b.y);

    bool IsInBounds(int2 pos) =>
        pos.x >= 0 && pos.x < gridSize.x &&
        pos.y >= 0 && pos.y < gridSize.y;

    bool IsWalkable(int2 pos) => !occupationCells[pos.x + pos.y * gridSize.x].isOccupied;
}
