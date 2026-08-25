using System.Collections.Generic;
using PipeQuest.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace PipeQuest.Grid
{
    public class GridManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 8;
        [SerializeField] private float cellSize = 80f;
        [SerializeField] private float cellSpacing = 4f;

        [Header("References")]
        [SerializeField] private RectTransform gridContainer;
        [SerializeField] private GameObject cellPrefab;

        [Header("Colors")]
        [SerializeField] private Color hiddenColor = new Color(0.93f, 0.93f, 0.93f);
        [SerializeField] private Color passableColor = new Color(0.78f, 0.90f, 0.79f);
        [SerializeField] private Color obstacleColor = new Color(0.26f, 0.26f, 0.26f);
        [SerializeField] private Color buildingColor = new Color(0.56f, 0.79f, 0.98f);
        [SerializeField] private Color sourceSeaColor = new Color(0.31f, 0.76f, 0.97f);
        [SerializeField] private Color sourceTankColor = new Color(1.0f, 0.54f, 0.40f);
        [SerializeField] private Color markerColor = new Color(1.0f, 0.92f, 0.23f);

        private CellData[,] gridData;
        private Cell[,] cells;
        private List<CellData> bluePlacedCells = new List<CellData>();
        private List<CellData> redPlacedCells = new List<CellData>();

        public static GridManager Instance { get; private set; }

        public int Width => gridWidth;
        public int Height => gridHeight;
        public float CellSize => cellSize;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeGrid();
        }

        public void InitializeGrid()
        {
            gridData = new CellData[gridWidth, gridHeight];
            cells = new Cell[gridWidth, gridHeight];
            GenerateRandomMap();
            BuildVisualGrid();
        }

        private void GenerateRandomMap()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    float rand = Random.value;
                    CellType type;

                    if (x == gridWidth / 2 && y == gridHeight - 1)
                        type = CellType.Building;
                    else if (x == 2 && y == 0)
                        type = CellType.SourceSea;
                    else if (x == gridWidth - 3 && y == 0)
                        type = CellType.SourceTank;
                    else if (rand < 0.15f)
                        type = CellType.Obstacle;
                    else
                        type = CellType.Passable;

                    gridData[x, y] = new CellData(x, y, type);
                }
            }

            EnsurePathExists(CellType.SourceSea, CellType.Building);
            EnsurePathExists(CellType.SourceTank, CellType.Building);
        }

        private void EnsurePathExists(CellType sourceType, CellType targetType)
        {
            CellData source = null;
            CellData target = null;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridData[x, y].type == sourceType) source = gridData[x, y];
                    if (gridData[x, y].type == targetType) target = gridData[x, y];
                }
            }

            if (source == null || target == null) return;

            List<CellData> path = FindPath(source, target);
            if (path == null)
            {
                ClearObstaclesBetween(source, target);
            }
        }

        private List<CellData> FindPath(CellData start, CellData end)
        {
            Queue<CellData> queue = new Queue<CellData>();
            HashSet<string> visited = new HashSet<string>();
            Dictionary<CellData, CellData> cameFrom = new Dictionary<CellData, CellData>();

            queue.Enqueue(start);
            visited.Add($"{start.x},{start.y}");

            while (queue.Count > 0)
            {
                CellData current = queue.Dequeue();

                if (current.x == end.x && current.y == end.y)
                {
                    List<CellData> path = new List<CellData>();
                    CellData step = end;
                    while (step != null)
                    {
                        path.Add(step);
                        if (!cameFrom.ContainsKey(step)) break;
                        step = cameFrom[step];
                    }
                    path.Reverse();
                    return path;
                }

                foreach (var neighbor in GetNeighbors(current))
                {
                    string key = $"{neighbor.x},{neighbor.y}";
                    if (!visited.Contains(key) && neighbor.type != CellType.Obstacle)
                    {
                        visited.Add(key);
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }
            return null;
        }

        private void ClearObstaclesBetween(CellData source, CellData target)
        {
            int midX = (source.x + target.x) / 2;
            for (int y = source.y; y <= target.y; y++)
            {
                if (gridData[midX, y].type == CellType.Obstacle)
                    gridData[midX, y].type = CellType.Passable;
            }
        }

        private List<CellData> GetNeighbors(CellData cell)
        {
            List<CellData> neighbors = new List<CellData>();
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nx = cell.x + dx[i];
                int ny = cell.y + dy[i];
                if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                {
                    neighbors.Add(gridData[nx, ny]);
                }
            }
            return neighbors;
        }

        private void BuildVisualGrid()
        {
            float totalWidth = gridWidth * cellSize + (gridWidth - 1) * cellSpacing;
            float totalHeight = gridHeight * cellSize + (gridHeight - 1) * cellSpacing;
            gridContainer.sizeDelta = new Vector2(totalWidth, totalHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                    RectTransform rt = cellObj.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(cellSize, cellSize);
                    rt.anchoredPosition = new Vector2(
                        x * (cellSize + cellSpacing),
                        y * (cellSize + cellSpacing)
                    );

                    Cell cell = cellObj.GetComponent<Cell>();
                    if (cell == null) cell = cellObj.AddComponent<Cell>();

                    cell.Initialize(gridData[x, y], GetColorForType(gridData[x, y].type));
                    cells[x, y] = cell;
                }
            }
        }

        private Color GetColorForType(CellType type)
        {
            switch (type)
            {
                case CellType.Hidden: return hiddenColor;
                case CellType.Passable: return passableColor;
                case CellType.Obstacle: return obstacleColor;
                case CellType.Building: return buildingColor;
                case CellType.SourceSea: return sourceSeaColor;
                case CellType.SourceTank: return sourceTankColor;
                default: return hiddenColor;
            }
        }

        public CellData GetCellData(int x, int y)
        {
            if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return null;
            return gridData[x, y];
        }

        public Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return null;
            return cells[x, y];
        }

        public void RevealAllCells()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    cells[x, y].Reveal(GetColorForType(gridData[x, y].type));
                }
            }
        }

        public void HideAllCells()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridData[x, y].type != CellType.Building &&
                        gridData[x, y].type != CellType.SourceSea &&
                        gridData[x, y].type != CellType.SourceTank)
                    {
                        cells[x, y].Hide(hiddenColor);
                    }
                }
            }
        }

        public void PlacePipe(int x, int y, PipeType pipeType, int rotation, PipeColor color)
        {
            CellData cell = GetCellData(x, y);
            if (cell == null) return;

            cell.placedPipe = pipeType;
            cell.pipeRotation = rotation;
            cell.pipeColor = color;

            if (color == PipeColor.Blue) bluePlacedCells.Add(cell);
            else redPlacedCells.Add(cell);

            cells[x, y].ShowPipe(pipeType, rotation, color);
        }

        public void RemovePipe(int x, int y)
        {
            CellData cell = GetCellData(x, y);
            if (cell == null || cell.placedPipe == PipeType.None) return;

            if (cell.pipeColor == PipeColor.Blue) bluePlacedCells.Remove(cell);
            else redPlacedCells.Remove(cell);

            cell.placedPipe = PipeType.None;
            cell.pipeRotation = 0;
            cells[x, y].RemovePipe();
        }

        public List<CellData> GetBluePlacedCells() => new List<CellData>(bluePlacedCells);
        public List<CellData> GetRedPlacedCells() => new List<CellData>(redPlacedCells);

        public void ClearAllPipes()
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (gridData[x, y].placedPipe != PipeType.None)
                    {
                        RemovePipe(x, y);
                    }
                }
            }
        }

        public void SetMarker(int x, int y, bool hasMarker)
        {
            CellData cell = GetCellData(x, y);
            if (cell == null) return;

            cell.hasMarker = hasMarker;
            cells[x, y].ShowMarker(hasMarker, markerColor);
        }

        public CellData GetSourceSea()
        {
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    if (gridData[x, y].type == CellType.SourceSea) return gridData[x, y];
            return null;
        }

        public CellData GetSourceTank()
        {
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    if (gridData[x, y].type == CellType.SourceTank) return gridData[x, y];
            return null;
        }

        public List<CellData> GetBuildings()
        {
            List<CellData> buildings = new List<CellData>();
            for (int x = 0; x < gridWidth; x++)
                for (int y = 0; y < gridHeight; y++)
                    if (gridData[x, y].type == CellType.Building) buildings.Add(gridData[x, y]);
            return buildings;
        }
    }
}
