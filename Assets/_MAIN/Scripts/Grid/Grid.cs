using System.Collections.Generic;
using System.Linq;
using NinovaGamesCase.Helpers;
using NinovaGamesCase.Managers;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    public enum Edge
    {
        Down = 0,
        Right = 1,
        Up = 2,
        Left = 3
    }

    /// <summary>
    /// Manages the grid system including nodes, doors, walls, blocks, and camera adjustment.
    /// </summary>
    public class Grid : Singleton<Grid>
    {
        private Node[,] _grid;
        public Node[,] GetGrid() => _grid;

        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private GameObject _doorPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private GameObject _blockPrefab;
        [SerializeField] private GameObject _cornerPrefab;
        [SerializeField] private GameObject _groundBGPrefab;

        private Vector3 _gridBottomLeft;

        public List<Block> AllSpawnedBlocks { get; private set; }
        private List<Door> _spawnedDoors = new();

        /// <summary>
        /// Clears all child objects in the grid and resets spawned blocks and doors.
        /// </summary>
        public void ClearGrid()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            AllSpawnedBlocks?.Clear();
            _spawnedDoors.Clear();
        }

        /// <summary>
        /// Creates the grid, including nodes, doors, blocks, corners, walls, ground background, and adjusts the camera.
        /// </summary>
        public void CreateGrid()
        {
            InitializeGridArray();
            CreateNodes();
            CreateDoors();
            CreateBlocks();
            CreateCorners();
            CreateGroundBackground();
            CreateWalls();
            AdjustCamera(Camera.main);
        }

        /// <summary>
        /// Initializes the _grid array and calculates the bottom-left position of the grid.
        /// </summary>
        private void InitializeGridArray()
        {
            var config = LevelManager.CurrentConfig;
            _grid = new Node[config.GridSize.x, config.GridSize.y];

            var center = Vector2.zero;
            var totalSize = new Vector2(config.GridSize.x * config.NodeSize.x, config.GridSize.y * config.NodeSize.y);

            var bottomLeft = center - totalSize / 2 + config.NodeSize / 2;
            _gridBottomLeft = new Vector3(bottomLeft.x, 0, bottomLeft.y);
        }

        /// <summary>
        /// Instantiates node gameobjects on the grid based on current configuration.
        /// </summary>
        private void CreateNodes()
        {
            var config = LevelManager.CurrentConfig;

            var origin = Vector2.zero;
            var totalSize = new Vector2(config.GridSize.x * config.NodeSize.x, config.GridSize.y * config.NodeSize.y);

            var bottomLeft = origin - new Vector2(totalSize.x, totalSize.y) / 2 +
                             new Vector2(config.NodeSize.x, config.NodeSize.y) / 2;
            
            _gridBottomLeft = new Vector3(bottomLeft.x, 0, bottomLeft.y);
            
            for (var x = 0; x < config.GridSize.x; x++)
            {
                for (var y = 0; y < config.GridSize.y; y++)
                {
                    var worldPosition = Helpers.Helpers.Vector2ToVector3(bottomLeft + new Vector2( x * LevelManager.CurrentConfig.NodeSize.x, y * LevelManager.CurrentConfig.NodeSize .y));
                    worldPosition.y = -.5f;
                    
                    var node = new Node(new Vector2Int(x, y), worldPosition);
                    _grid[x, y] = node;

                    var nodeClone = Instantiate(_nodePrefab, transform);
                    nodeClone.transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(90, 0, 0));
                    nodeClone.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Instantiates door gameobjects from level door data.
        /// </summary>
        private void CreateDoors()
        {
            _spawnedDoors.Clear();
            foreach (var levelDoorData in LevelManager.CurrentConfig.LevelDoorData)
            {
                var doorClone = Instantiate(_doorPrefab, transform);
                var door = doorClone.GetComponent<Door>();
                door.Initialize(levelDoorData);
                _spawnedDoors.Add(door);
                doorClone.SetActive(true);
            }
        }

        /// <summary>
        /// Instantiates block gameobjects from level block data.
        /// </summary>
        private void CreateBlocks()
        {
            AllSpawnedBlocks = new List<Block>();
            foreach (var data in LevelManager.CurrentConfig.LevelBlockData)
            {
                var blockClone = Instantiate(_blockPrefab, transform);
                var block = blockClone.GetComponent<Block>();
                block.Initialize(data);
                AllSpawnedBlocks.Add(block);
                blockClone.SetActive(true);
            }
        }

        /// <summary>
        /// Instantiates corner gameobjects at each corner of the grid with appropriate position and rotation.
        /// </summary>
        private void CreateCorners()
        {
            var config = LevelManager.CurrentConfig;
            for (var i = 0; i < 4; i++)
            {
                var cornerClone = Instantiate(_cornerPrefab, transform);
                var pos = Vector3.zero;
                var rot = Quaternion.identity;

                switch (i)
                {
                    case 0:
                        pos = _grid[0, 0].WorldPosition + Vector3.left + Vector3.back;
                        rot = Quaternion.Euler(0, 270f, 0);
                        break;
                    case 1:
                        pos = _grid[0, config.GridSize.y - 1].WorldPosition + Vector3.left + Vector3.forward;
                        break;
                    case 2:
                        pos = _grid[config.GridSize.x - 1, 0].WorldPosition + Vector3.right + Vector3.back;
                        rot = Quaternion.Euler(0, 180f, 0);
                        break;
                    case 3:
                        pos = _grid[config.GridSize.x - 1, config.GridSize.y - 1].WorldPosition + Vector3.right +
                              Vector3.forward;
                        rot = Quaternion.Euler(0, 90f, 0);
                        break;
                }

                pos.y = 0;
                cornerClone.transform.position = pos;
                cornerClone.transform.rotation = rot;
                cornerClone.SetActive(true);
            }
        }

        /// <summary>
        /// Instantiates the ground background gameobject and adjusts its scale based on grid size.
        /// </summary>
        private void CreateGroundBackground()
        {
            var config = LevelManager.CurrentConfig;
            var groundBGClone = Instantiate(_groundBGPrefab, transform);
            var groundScaleX = 0.7f + 0.1f * (config.GridSize.x - 3);
            var groundScaleY = 0.7f + 0.1f * (config.GridSize.y - 3);
            groundBGClone.transform.localScale = new Vector3(groundScaleX, groundScaleY);
            groundBGClone.SetActive(true);
        }

        /// <summary>
        /// Instantiates walls along grid edges, excluding door positions.
        /// </summary>
        private void CreateWalls()
        {
            for (var i = 0; i < 4; i++)
            {
                var edge = (Edge)i;
                var wallGroups = GetWallGroupsForEdge(edge);
                var direction = EdgeToLevelDoorDirection(edge);

                foreach (var nodes in wallGroups)
                {
                    var wallClone = Instantiate(_wallPrefab, transform);
                    var levelWallData = new LevelWallData
                    {
                        Direction = direction,
                        GridPosition = nodes[0].GridPosition,
                        Length = nodes.Count
                    };
                    wallClone.GetComponent<Wall>().Initialize(levelWallData);
                    wallClone.SetActive(true);
                }
            }
        }


        /// <summary>
        /// Returns grouped nodes along a given edge that do not contain doors, used to create wall segments.
        /// </summary>
        /// <param name="edge">Edge enum indicating which edge to process.</param>
        /// <returns>List of node groups without doors on the specified edge.</returns>
        private List<List<Node>> GetWallGroupsForEdge(Edge edge)
        {
            var groups = new List<List<Node>>();
            var limit = (edge == Edge.Down || edge == Edge.Up)
                ? LevelManager.CurrentConfig.GridSize.x
                : LevelManager.CurrentConfig.GridSize.y;

            var s = 0;
            while (s < limit)
            {
                var currentGroup = new List<Node>();

                for (var j = s; j < limit; j++)
                {
                    var node = edge switch
                    {
                        Edge.Down => _grid[j, 0],
                        Edge.Right => _grid[LevelManager.CurrentConfig.GridSize.x - 1, j],
                        Edge.Up => _grid[j, LevelManager.CurrentConfig.GridSize.y - 1],
                        Edge.Left => _grid[0, j],
                        _ => null
                    };

                    var isDoorNode = _spawnedDoors.Any(door =>
                        door.CurrentNodes.Contains(node) &&
                        GetEdgeFromDirection(door.LevelDoorData.Direction) == edge);

                    if (isDoorNode)
                    {
                        s = j + 1;
                        break;
                    }

                    currentGroup.Add(node);
                    s = j + 1;
                }

                if (currentGroup.Count > 0)
                    groups.Add(currentGroup);
            }

            return groups;
        }

        /// <summary>
        /// Converts a LevelDoorDirection enum to an Edge enum.
        /// </summary>
        /// <param name="direction">The door direction.</param>
        /// <returns>Corresponding Edge enum value.</returns>
        private Edge GetEdgeFromDirection(LevelDoorDirection direction) => direction switch
        {
            LevelDoorDirection.Down => Edge.Down,
            LevelDoorDirection.Right => Edge.Right,
            LevelDoorDirection.Up => Edge.Up,
            LevelDoorDirection.Left => Edge.Left,
            _ => Edge.Down
        };

        /// Converts an Edge enum to a LevelDoorDirection enum.
        /// </summary>
        /// <param name="edge">The grid edge.</param>
        /// <returns>Corresponding LevelDoorDirection enum value.</returns>
        private LevelDoorDirection EdgeToLevelDoorDirection(Edge edge) => edge switch
        {
            Edge.Down => LevelDoorDirection.Down,
            Edge.Right => LevelDoorDirection.Right,
            Edge.Up => LevelDoorDirection.Up,
            Edge.Left => LevelDoorDirection.Left,
            _ => LevelDoorDirection.Down
        };

        /// <summary>
        /// Adjusts the main camera position and rotation based on grid size.
        /// </summary>
        private void AdjustCamera(Camera cam)
        {
            var config = LevelManager.CurrentConfig;
            var gridSize = config.GridSize;
            var nodeSize = config.NodeSize;

            var totalSize = new Vector2(gridSize.x * nodeSize.x, gridSize.y * nodeSize.y);
            var gridCenter = _gridBottomLeft + new Vector3(totalSize.x / 2f, 0f, totalSize.y / 2f);

            const float refSize = 3f;
            const float refY = 12f;
            const float refZ = -4f;

            var sizeFactor = Mathf.Sqrt(Mathf.Max(gridSize.x, gridSize.y) / refSize);

            var newPos = new Vector3(
                0,
                refY * sizeFactor,
                gridCenter.z + refZ * sizeFactor
            );

            cam.transform.position = newPos;
            cam.transform.rotation = Quaternion.Euler(75f, 0f, 0f);
        }

        /// <summary>
        /// Finds the closest available node (without a block) to the given world position.
        /// </summary>
        public Node GetClosestAvailableNode(Vector3 worldPos)
        {
            Node closest = null;
            var closestDist = float.MaxValue;

            foreach (var node in _grid)
            {
                if (node.Block != null) continue;

                var dist = Vector3.Distance(worldPos, node.WorldPosition);
                if (dist < closestDist)
                {
                    closest = node;
                    closestDist = dist;
                }
            }

            return closest;
        }

        /// <summary>
        /// Snaps a world position to the nearest node in the grid, or returns null if out of bounds.
        /// </summary>
        public Node GetSnappedNode(Vector3 pos)
        {
            var nodeSize = LevelManager.CurrentConfig.NodeSize;

            var snappedX = Mathf.RoundToInt(pos.x / nodeSize.x) * (int)nodeSize.x;
            var snappedZ = Mathf.RoundToInt(pos.z / nodeSize.y) * (int)nodeSize.y;

            var gridX = Mathf.RoundToInt((snappedX - _gridBottomLeft.x) / nodeSize.x);
            var gridY = Mathf.RoundToInt((snappedZ - _gridBottomLeft.z) / nodeSize.y);

            if (!IsNodeInBounds(new Vector2Int(gridX, gridY))) return null;

            return _grid[gridX, gridY];
        }

        /// <summary>
        /// Checks if a grid position is within the grid bounds.
        /// </summary>
        public bool IsNodeInBounds(Vector2Int pos) =>
            pos.x >= 0 && pos.x < LevelManager.CurrentConfig.GridSize.x &&
            pos.y >= 0 && pos.y < LevelManager.CurrentConfig.GridSize.y;

        /// <summary>
        /// Returns all spawned blocks that match a specified color.
        /// </summary>
        public List<Block> GetAllBlockByColor(MatchColor color) =>
            AllSpawnedBlocks.FindAll(block => block.BlockData.ColorData.MatchColor == color);
    }
}