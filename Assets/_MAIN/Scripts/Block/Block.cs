using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HighlightPlus;
using NinovaGamesCase.Helpers;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    [System.Serializable]
    public struct ColorData
    {
        public string Color;
        public Material ColorMaterial;
    }
    
    [System.Serializable]
    public struct BlockData
    {
        public ColorData ColorData;
    }
    
    public class Block : MonoBehaviour
    {
        public static event Action OnMoved;
        public static event Action OnMoveFailed;
        public static event Action<Door, Block> OnMatched;
        
        private BlockData BlockData;
        private List<Node> Nodes = new();
        private List<Node> _tempNodes = new();
        private List<BlockMesh> _blockMeshes = new();

        [SerializeField] private GameObject _blockMeshPrefab;
        private HighlightEffect _highlightEffect;

        private bool _active = true;

        private void Awake()
        {
            _highlightEffect = GetComponent<HighlightEffect>();
        }

        public void Initialize(LevelBlockData data)
        {
            BlockData = data.BlockData;
            
            var node = Grid.Instance.GetGrid()[data.GridPosition.x, data.GridPosition.y];
            transform.position = node.WorldPosition;
            Nodes.Add(node);
            foreach (var pos in data.MeshPositions)
            {
                node = Grid.Instance.GetGrid()[data.GridPosition.x + pos.x, data.GridPosition.y + pos.y];
                var meshClone = Instantiate(_blockMeshPrefab, transform);
                meshClone.transform.position = node.WorldPosition;
                meshClone.SetActive(true);
                Nodes.Add(node);
            }

            _blockMeshes = GetComponentsInChildren<BlockMesh>().ToList();

            foreach (var _node in Nodes)
                _node.SetBlock(this);

            foreach (var mesh in _blockMeshes)
                mesh.SetColor(BlockData.ColorData.ColorMaterial);
        }
        
        
      
        
        public void StartDrag()
        {
            if (!_active)
                return;
            
            OutlineShader(true);
            
            ResetNodes(true);
            
            Nodes.Clear();
        }

        private void ResetNodes(bool save)
        {
            if(save) _tempNodes = Nodes.ToList();
            else _tempNodes.Clear();
            
            foreach (var node in Nodes)
                node.SetBlock(null);
            
            Nodes.Clear();
        }

        public Vector3 GetPosition(Vector3 mPos, Vector3 meshPos)
        {
            var p = meshPos - transform.position;
            return mPos - p;
        }
        
        public void Dragging(Vector3 pos, Vector3 meshPos)
        {
            if (!_active)
                return;
            
            var p = GetPosition(pos, meshPos) - transform.position;
            transform.position += p;   


            var pp = transform.position;
            pp.y = .1f;
            transform.position = pp;
        }
        

        #region DROP

        public void Drop(Node node)
        {
            if (!_active)
                return;
            
            if (node == null || !TryGetValidNodes(node, out var validNodes))
            {
                ResetToTempNodes();
            }
            else
            {
                Nodes = validNodes;
                foreach (var n in Nodes)
                    n.SetBlock(this);

                transform.position = Nodes.First().WorldPosition;
                OnMoved?.Invoke();

                CheckDoorMatch();
            }

            OutlineShader(false);
        }
        
        private void ResetToTempNodes()
        {
            Nodes = _tempNodes;
            foreach (var node in Nodes)
                node.SetBlock(this);

            transform.position = Nodes.First().WorldPosition;
            
            OnMoveFailed?.Invoke();
        }
        
        private bool TryGetValidNodes(Node pivotNode, out List<Node> result)
        {
            result = new List<Node>();
            var gridPositions = new List<Vector2Int>
            {
                pivotNode.GridPosition
            };

            for (var i = 1; i < _blockMeshes.Count; i++)
            {
                var offset = _blockMeshes[i].transform.localPosition;
                var gridPos = pivotNode.GridPosition + new Vector2Int((int)offset.x, (int)offset.z);
                gridPositions.Add(gridPos);
            }

            foreach (var pos in gridPositions)
            {
                if (!Grid.Instance.IsNodeInBounds(pos))
                    return false;

                var node = Grid.Instance.GetGrid()[pos.x, pos.y];
                if (node == null || node.Block)
                    return false;

                result.Add(node);
            }

            return true;
        }

        #endregion
        
        #region CHECK_DOOR_MATCH

        private void CheckDoorMatch()
        {
            var matchedDoor = FindMatchingDoor();

            if (matchedDoor == null)
                return;
            
            var uniqueLines = GetUniqueLineCount(matchedDoor.LevelDoorData.Direction);
            if (uniqueLines.Count <= matchedDoor.LevelDoorData.Length)
            {
                if (matchedDoor.LevelDoorData.Direction == LevelDoorDirection.Up ||
                    matchedDoor.LevelDoorData.Direction == LevelDoorDirection.Down)
                {
                    foreach (var line in uniqueLines)
                    {
                        var s = false;
                        foreach (var node in matchedDoor.CurrentNodes)
                        {
                            if ((int)node.WorldPosition.x == line)
                            {
                                s = true;
                                break;
                            }
                        }

                        if (!s)
                            return;
                    }
                }
                else
                {
                    foreach (var line in uniqueLines)
                    {
                        var s = false;
                        foreach (var node in matchedDoor.CurrentNodes)
                        {
                            if ((int)node.WorldPosition.z == line)
                            {
                                s = true;
                                break;
                            }
                        }

                        if (!s)
                            return;
                    }
                }
                
                Debug.Log("Matched");
                _active = false;
                ResetNodes(false);
                OnMatched?.Invoke(matchedDoor, this);
            }
        }
        
        private Door FindMatchingDoor()
        {
            foreach (var mesh in _blockMeshes)
            {
                var door = mesh.CheckDoor();
                if (door && door.LevelDoorData.ColorData.Color == BlockData.ColorData.Color)
                {
                    Debug.Log(BlockData.ColorData.Color);
                    return door;
                }
            }

            return null;
        }
        
        
        private HashSet<int> GetUniqueLineCount(LevelDoorDirection direction)
        {
            HashSet<int> uniqueLinesAmount = new();


            foreach (Transform child in transform)
            {
                var key = (direction == LevelDoorDirection.Up || direction == LevelDoorDirection.Down)
                    ? Mathf.RoundToInt(child.position.x)
                    : Mathf.RoundToInt(child.position.z);

                uniqueLinesAmount.Add(key);
            }

            return uniqueLinesAmount;
        }

        #endregion
     
        
        private void OutlineShader(bool state) => _highlightEffect.highlighted = state;

        public void OnMatchedMove(Vector3 pos)
        {
            pos.y = transform.position.y;
            transform.DOMove(pos, 2f).SetEase(Ease.Linear);
        }
    }   
}
