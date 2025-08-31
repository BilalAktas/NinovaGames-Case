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

        private List<Door> _spawnedDoors = new();


        public void ClearGrid()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
        
        public void CreateGrid()
        {
            _grid = new Node[LevelManager.CurrentConfig.GridSize.x, LevelManager.CurrentConfig.GridSize.y];

            var center = Vector2.zero;
            var totalSize = new Vector2(LevelManager.CurrentConfig.GridSize.x * LevelManager.CurrentConfig.NodeSize.x,
                LevelManager.CurrentConfig.GridSize.y * LevelManager.CurrentConfig.NodeSize.y);

            var bottomLeft = center - new Vector2(totalSize.x, totalSize.y) / 2 + new Vector2(LevelManager.CurrentConfig.NodeSize.x
                , LevelManager.CurrentConfig.NodeSize.y) / 2;

            for (var x = 0; x < LevelManager.CurrentConfig.GridSize.x; x++)
            {
                for (var y = 0; y < LevelManager.CurrentConfig.GridSize.y; y++)
                {
                    var worldPosition = Helpers.Helpers.Vector2ToVector3(bottomLeft +
                                                                         new Vector2(x * LevelManager.CurrentConfig.NodeSize.x,
                                                                             y * LevelManager.CurrentConfig.NodeSize.y));
                    worldPosition.y = -.5f;
                    var gridPosition = new Vector2Int(x, y);
                    var node = new Node(gridPosition, worldPosition);
                    _grid[x, y] = node;

                    var nodeClone = Instantiate(_nodePrefab, transform);
                    nodeClone.transform.SetPositionAndRotation(worldPosition, Quaternion.Euler(90, 0, 0));
                    nodeClone.SetActive(true);
                }
            }

            foreach (var levelDoorData in LevelManager.CurrentConfig.LevelDoorData)
            {
                var doorClone = Instantiate(_doorPrefab, transform);
                var doorS = doorClone.GetComponent<Door>();
                doorS.Initialize(levelDoorData);
                _spawnedDoors.Add(doorS);
                doorClone.SetActive(true);
            }


            foreach (var data in LevelManager.CurrentConfig.LevelBlockData)
            {
                var blockClone = Instantiate(_blockPrefab, transform);
                blockClone.GetComponent<Block>().Initialize(data);
                blockClone.SetActive(true);
            }

            for (var i = 0; i < 4; i++)
            {
                var cornerClone = Instantiate(_cornerPrefab, transform);
                var pos = Vector3.zero;
                var rot = Quaternion.identity;

                switch (i)
                {
                    case 0:
                        var node = _grid[0, 0];
                        pos = node.WorldPosition + (Vector3.left * 1 + Vector3.back * 1);
                        rot = Quaternion.Euler(0, 270f, 0);

                        break;
                    case 1:
                        var node1 = _grid[0, LevelManager.CurrentConfig.GridSize.y - 1];
                        pos = node1.WorldPosition + (Vector3.left * 1 + Vector3.forward * 1);
                        break;

                    case 2:
                        var node2 = _grid[LevelManager.CurrentConfig.GridSize.x - 1, 0];
                        pos = node2.WorldPosition + (Vector3.right * 1 + Vector3.back * 1);
                        rot = Quaternion.Euler(0, 180f, 0);
                        break;

                    case 3:
                        var node3 = _grid[LevelManager.CurrentConfig.GridSize.x - 1, LevelManager.CurrentConfig.GridSize.y - 1];
                        pos = node3.WorldPosition + (Vector3.right * 1 + Vector3.forward * 1);
                        rot = Quaternion.Euler(0, 90f, 0);
                        break;
                }

                pos.y = 0;
                cornerClone.transform.position = pos;
                cornerClone.transform.rotation = rot;
                cornerClone.SetActive(true);
            }
            
            var groundBGClone = Instantiate(_groundBGPrefab, transform);
            var groundScaleX = .7f + (.1f * (LevelManager.CurrentConfig.GridSize.x - 3));
            var groundScaleY = .7f + (.1f * (LevelManager.CurrentConfig.GridSize.y - 3));
            groundBGClone.transform.localScale = new Vector3(groundScaleX, groundScaleY);
            groundBGClone.SetActive(true);
            
            
            CreateWalls();
        }
        
           private void CreateWalls()
            {
                for (var i = 0; i < 4; i++)
                {
                    var edge = (Edge)i;
                    var groupedNodes = GetWallGroupsForEdge(edge);

                    var direction = edge switch
                    {
                        Edge.Down => LevelDoorDirection.Down,
                        Edge.Right => LevelDoorDirection.Right,
                        Edge.Up => LevelDoorDirection.Up,
                        Edge.Left => LevelDoorDirection.Left,
                        _ => LevelDoorDirection.Down
                    };

                    foreach (var nodes in groupedNodes)
                    {
                        var wallClone = Instantiate(_wallPrefab, transform);
                        var levelWallData = new LevelWallData()
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

            private List<List<Node>> GetWallGroupsForEdge(Edge edge)
            {
                var groupedNodes = new List<List<Node>>();
                var currentGroup = new List<Node>();

                var limit = (edge == Edge.Down || edge == Edge.Up) ? LevelManager.CurrentConfig.GridSize.x : LevelManager.CurrentConfig.GridSize.y;

                for (var s = 0; s < limit;)
                {
                    var checkDoor = false;

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
                        
                        foreach (var door in _spawnedDoors)
                        {
                            if (door.CurrentNodes.Contains(node))
                            {
                                checkDoor = true;
                                break;
                            }
                        }

                        s++;

                        if (checkDoor)
                            break;

                        currentGroup.Add(node);
                    }

                    if (currentGroup.Count > 0)
                    {
                        groupedNodes.Add(currentGroup);
                        currentGroup = new List<Node>();
                    }
                }

                return groupedNodes;
            }

        public Node GetSnappedNode(Vector3 pos)
        {
            var m_GridSize = new Vector3(LevelManager.CurrentConfig.NodeSize.x, 0, LevelManager.CurrentConfig.NodeSize.y);
            var p = new Vector3(
                Mathf.RoundToInt(pos.x / m_GridSize.x) * m_GridSize.x,
                Mathf.RoundToInt(pos.y / m_GridSize.y) * m_GridSize.y,
                Mathf.RoundToInt(pos.z / m_GridSize.z) * m_GridSize.z);

            p.y = -.5f;
            foreach (var node in _grid)
            {
                if (node.WorldPosition == p)
                    return node;
            }


            return null;
        }

        public bool IsNodeInBounds(Vector2Int pos) =>
            pos.x >= 0 && pos.x < LevelManager.CurrentConfig.GridSize.x && pos.y >= 0 && pos.y < LevelManager.CurrentConfig.GridSize.y;
    }
}