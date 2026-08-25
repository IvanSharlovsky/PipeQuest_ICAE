using System.Collections.Generic;
using PipeQuest.Core.Enums;
using PipeQuest.Grid;
using UnityEngine;

namespace PipeQuest.Pipes

{
    public class PipeValidator : MonoBehaviour
    {
        [System.Serializable]
        public class ValidationResult
        {
            public bool isValid;
            public string errorMessage;
            public Vector2Int errorPosition;
            public List<CellData> validPath;
            public bool hasIntersection;
        }

        public ValidationResult ValidateSystem(PipeColor color)
        {
            ValidationResult result = new ValidationResult();

            CellData source = color == PipeColor.Blue
                ? Grid.GridManager.Instance.GetSourceSea()
                : Grid.GridManager.Instance.GetSourceTank();

            List<CellData> targets = Grid.GridManager.Instance.GetBuildings();
            List<CellData> placedCells = color == PipeColor.Blue
                ? Grid.GridManager.Instance.GetBluePlacedCells()
                : Grid.GridManager.Instance.GetRedPlacedCells();

            if (source == null || placedCells.Count == 0)
            {
                result.isValid = false;
                result.errorMessage = "Нет установленных труб";
                return result;
            }

            List<CellData> path = BFS(source, placedCells, color);

            if (path == null || path.Count == 0)
            {
                result.isValid = false;
                result.errorMessage = "Разрыв в трубе";
                result.errorPosition = FindBreakPoint(source, placedCells, color);
                return result;
            }

            bool reachedTarget = false;
            foreach (var target in targets)
            {
                if (path.Exists(p => p.x == target.x && p.y == target.y))
                {
                    reachedTarget = true;
                    break;
                }
            }

            if (!reachedTarget)
            {
                result.isValid = false;
                result.errorMessage = "Труба не доходит до здания АЭС";
                result.errorPosition = new Vector2Int(path[path.Count - 1].x, path[path.Count - 1].y);
                return result;
            }

            result.isValid = true;
            result.validPath = path;
            return result;
        }

        public bool CheckIntersections()
        {
            List<CellData> blueCells = Grid.GridManager.Instance.GetBluePlacedCells();
            List<CellData> redCells = Grid.GridManager.Instance.GetRedPlacedCells();

            foreach (var blue in blueCells)
            {
                foreach (var red in redCells)
                {
                    if (blue.x == red.x && blue.y == red.y) return true;
                }
            }
            return false;
        }

        private List<CellData> BFS(CellData source, List<CellData> placedCells, PipeColor color)
        {
            Queue<CellData> queue = new Queue<CellData>();
            HashSet<string> visited = new HashSet<string>();
            Dictionary<CellData, CellData> cameFrom = new Dictionary<CellData, CellData>();

            queue.Enqueue(source);
            visited.Add($"{source.x},{source.y}");

            CellData lastValid = source;

            while (queue.Count > 0)
            {
                CellData current = queue.Dequeue();
                lastValid = current;

                if (current.type == CellType.Building && current.pipeColor == color)
                {
                    return ReconstructPath(cameFrom, source, current);
                }

                foreach (var neighbor in GetConnectedNeighbors(current, color))
                {
                    string key = $"{neighbor.x},{neighbor.y}";
                    if (!visited.Contains(key) && placedCells.Exists(p => p.x == neighbor.x && p.y == neighbor.y))
                    {
                        visited.Add(key);
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return ReconstructPath(cameFrom, source, lastValid);
        }

        private List<CellData> ReconstructPath(Dictionary<CellData, CellData> cameFrom, CellData start, CellData end)
        {
            List<CellData> path = new List<CellData>();
            CellData current = end;
            int safety = 0;

            while (current != null && safety < 200)
            {
                safety++;
                path.Add(current);
                if (current.x == start.x && current.y == start.y) break;
                if (!cameFrom.ContainsKey(current)) break;
                current = cameFrom[current];
            }

            path.Reverse();
            return path;
        }

        private Vector2Int FindBreakPoint(CellData source, List<CellData> placedCells, PipeColor color)
        {
            Queue<CellData> queue = new Queue<CellData>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(source);
            visited.Add($"{source.x},{source.y}");

            CellData lastReachable = source;

            while (queue.Count > 0)
            {
                CellData current = queue.Dequeue();
                lastReachable = current;

                foreach (var neighbor in GetConnectedNeighbors(current, color))
                {
                    string key = $"{neighbor.x},{neighbor.y}";
                    if (!visited.Contains(key) && placedCells.Exists(p => p.x == neighbor.x && p.y == neighbor.y))
                    {
                        visited.Add(key);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return new Vector2Int(lastReachable.x, lastReachable.y);
        }

        private List<CellData> GetConnectedNeighbors(CellData cell, PipeColor color)
        {
            List<CellData> neighbors = new List<CellData>();
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            int[] pipeConnections = GetPipeConnections(cell.placedPipe, cell.pipeRotation);

            for (int i = 0; i < 4; i++)
            {
                if (pipeConnections[i] == 0) continue;

                int nx = cell.x + dx[i];
                int ny = cell.y + dy[i];

                CellData neighbor = Grid.GridManager.Instance.GetCellData(nx, ny);
                if (neighbor == null) continue;

                if (neighbor.placedPipe == PipeType.None && neighbor.type != CellType.Building) continue;

                if (neighbor.type == CellType.Building && neighbor.pipeColor != color && neighbor.pipeColor != PipeColor.Blue)
                    continue;

                int[] neighborConnections = GetPipeConnections(neighbor.placedPipe, neighbor.pipeRotation);
                int oppositeDir = (i + 2) % 4;

                if (neighborConnections[oppositeDir] == 1 || neighbor.type == CellType.Building)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        private int[] GetPipeConnections(PipeType type, int rotation)
        {
            int[] connections = new int[4];

            switch (type)
            {
                case PipeType.Straight: connections = new int[] { 1, 0, 1, 0 }; break;
                case PipeType.Elbow: connections = new int[] { 1, 1, 0, 0 }; break;
                case PipeType.Tee: connections = new int[] { 1, 1, 1, 0 }; break;
                case PipeType.Valve: connections = new int[] { 1, 1, 1, 1 }; break;
                default: connections = new int[] { 1, 1, 1, 1 }; break;
            }

            int rotations = rotation / 90;
            for (int r = 0; r < rotations; r++)
            {
                int temp = connections[3];
                for (int i = 3; i > 0; i--)
                    connections[i] = connections[i - 1];
                connections[0] = temp;
            }

            return connections;
        }
    }
}