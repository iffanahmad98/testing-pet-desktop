using UnityEngine;
using System.Collections.Generic;
using System;
using MagicalGarden.Manager;
using MagicalGarden.Farm;

namespace MagicalGarden.AI
{
    public class AStarPathfinder
    {
        private HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        private PriorityQueue<Vector2Int> openSet = new PriorityQueue<Vector2Int>();
        private Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        private Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();

        private System.Func<Vector2Int, bool> isWalkable;

        public AStarPathfinder(System.Func<Vector2Int, bool> isWalkableCheck)
        {
            isWalkable = isWalkableCheck;
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            closedSet.Clear();
            openSet.Clear();
            cameFrom.Clear();
            gScore.Clear();

            openSet.Enqueue(start, 0);
            gScore[start] = 0;

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current == goal)
                    return ReconstructPath(current);

                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor) || !isWalkable(neighbor)) continue;

                    float tentativeG = gScore[current] + 1f;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        float fScore = tentativeG + Heuristic(neighbor, goal);
                        openSet.Enqueue(neighbor, fScore);
                    }
                }
            }

            return null; // No path
        }

        private List<Vector2Int> ReconstructPath(Vector2Int current)
        {
            List<Vector2Int> totalPath = new List<Vector2Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }
            return totalPath;
        }

        private float Heuristic(Vector2Int a, Vector2Int b)
        {
            float distance = Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
            if (distance % 2 == 0)
                return distance * 1.1f; 
            return distance;
        }

        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            // Neighbor biasa (4 arah)
            yield return pos + Vector2Int.up;
            yield return pos + Vector2Int.down;
            yield return pos + Vector2Int.left;
            yield return pos + Vector2Int.right;

            // Neighbor lompat (2 tile lurus)
            if (CanJumpOver(pos, pos + Vector2Int.up * 2)) yield return pos + Vector2Int.up * 2;
            if (CanJumpOver(pos, pos + Vector2Int.down * 2)) yield return pos + Vector2Int.down * 2;
            if (CanJumpOver(pos, pos + Vector2Int.left * 2)) yield return pos + Vector2Int.left * 2;
            if (CanJumpOver(pos, pos + Vector2Int.right * 2)) yield return pos + Vector2Int.right * 2;
        }
        protected virtual bool IsWalkableTile(Vector2Int tileCoord)
        {
            Vector3Int gridPos = new Vector3Int(tileCoord.x, tileCoord.y, 0);
            var tile = TileManager.Instance.tilemapSoil.GetTile(gridPos);
            if (tile is CustomTile myTile)
            {
                return myTile.tileType == TileType.Walkable;
            }
            return false;
        }

        private bool CanJumpOver(Vector2Int from, Vector2Int to)
        {
            Vector2Int delta = to - from;

            // Hanya izinkan lompat 2 tile lurus (tidak diagonal)
            if ((Mathf.Abs(delta.x) == 2 && delta.y == 0) ||
                (Mathf.Abs(delta.y) == 2 && delta.x == 0))
            {
                Vector2Int middle = from + new Vector2Int(delta.x / 2, delta.y / 2);
                return !IsWalkableTile(middle) && IsWalkableTile(to);
            }
            return false;
        }
    }
    public class PriorityQueue<T>
    {
        private readonly List<(T item, float priority)> elements = new();
        private readonly Dictionary<T, int> itemIndices = new();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            if (itemIndices.TryGetValue(item, out int index))
            {
                // Update existing priority if better
                if (priority < elements[index].priority)
                {
                    elements[index] = (item, priority);
                    BubbleUp(index);
                }
                return;
            }

            elements.Add((item, priority));
            itemIndices[item] = elements.Count - 1;
            BubbleUp(elements.Count - 1);
        }

        public T Dequeue()
        {
            if (elements.Count == 0) throw new InvalidOperationException("Queue is empty");

            var top = elements[0];
            itemIndices.Remove(top.item);

            if (elements.Count > 1)
            {
                elements[0] = elements[^1];
                itemIndices[elements[0].item] = 0;
                elements.RemoveAt(elements.Count - 1);
                BubbleDown(0);
            }
            else
            {
                elements.Clear();
            }

            return top.item;
        }

        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (elements[parent].priority <= elements[index].priority) break;

                Swap(index, parent);
                index = parent;
            }
        }

        private void BubbleDown(int index)
        {
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;

                if (left < elements.Count && elements[left].priority < elements[smallest].priority)
                    smallest = left;
                if (right < elements.Count && elements[right].priority < elements[smallest].priority)
                    smallest = right;

                if (smallest == index) break;

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            (elements[i], elements[j]) = (elements[j], elements[i]);
            itemIndices[elements[i].item] = i;
            itemIndices[elements[j].item] = j;
        }

        public bool Contains(T item) => itemIndices.ContainsKey(item);
        public void Clear()
        {
            elements.Clear();
            itemIndices.Clear();
        }
    }
}