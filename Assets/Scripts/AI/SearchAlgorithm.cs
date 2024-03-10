using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
namespace Utils
{
    class SearchAlgorithm
    {
        private PathNode[,] grid = null;
        public SearchAlgorithm(PathNode[,] grid)
        {
            this.grid = grid;
        }

        private List<Vector2Int> GetNeighbours(Vector2Int current)
        {
            List<Vector2Int> nodes = new List<Vector2Int>();
            for (int x = current.x - 1; x <= current.x + 1; ++x)
                for (int y = current.y - 1; y <= current.y + 1; ++y)
                    if (x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1) && (x != current.x || y != current.y))
                        nodes.Add(new Vector2Int(x, y));
            return nodes;
        }

        private Vector3 GetCoordinates(Vector2Int gridNode)
        {
            var node = grid[gridNode.x, gridNode.y];
            return new Vector3(node.body.transform.position.x,
                               node.body.transform.position.y,
                               node.body.transform.position.z);
        }

        private Vector3 FindDeltas(Vector3 start, Vector3 end)
        {
            return new Vector3(Mathf.Abs(start.x - end.x),
                               Mathf.Abs(start.y - end.y),
                               Mathf.Abs(start.z - end.z));
        }
        public float EuclidianDistance(Vector2Int current, Vector2Int finish, float heightWeight = 1)
        {
            var deltas = FindDeltas(GetCoordinates(current), GetCoordinates(finish));
            var weightedHeight = heightWeight * deltas.y;
            return Mathf.Sqrt(deltas.x * deltas.x + deltas.z * deltas.z + weightedHeight * weightedHeight);
        }

        public PathNode[,] Algorithm(Vector2Int startNode, Vector2Int finishNode, Color color, Func<Vector2Int, Vector2Int, float,float> dist=null)
        
        {
            //  ќчередь вершин в обработке - в A* необходимо заменить на очередь с приоритетом
            var nodes = new PriorityQueue<Vector2Int, float>();
            var came_from = new Dictionary<Vector2Int, Vector2Int>();
            var cost_so_far = new Dictionary<Vector2Int, float>();
            var current = new Vector2Int(0, 0);

            nodes.Enqueue(startNode, 0);
            came_from.Add(startNode, startNode);
            cost_so_far.Add(startNode, 0);
            //  ѕока не обработаны все вершины (очередь содержит узлы дл€ обработки)
            while (nodes.Count > 0)
            {
                current = nodes.Dequeue();
                //  ≈сли достали целевую - можно заканчивать (это верно и дл€ A*)
                if (current == finishNode) break;
                //  ѕолучаем список соседей
                var neighbours = GetNeighbours(current);
                foreach (var node in neighbours)
                {
                    if (!grid[node.x, node.y].walkable) continue;
                    float new_cost = cost_so_far[current] + PathNode.Dist(grid[node.x, node.y], grid[current.x, current.y]);
                    if (!cost_so_far.ContainsKey(node) || new_cost < cost_so_far[node])
                    {
                        cost_so_far[node] = new_cost;
                        float priority = new_cost + dist(finishNode, node, 5);
                        nodes.Enqueue(node, priority);
                        came_from[node] = current;
                    }
                }
            }

            while (current != startNode)
            {
                grid[current.x, current.y].Illuminate(color);
                current = came_from[current];
            }
            return grid;
        }
    }


}