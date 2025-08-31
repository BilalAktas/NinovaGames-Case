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
        public static event Action<Door, Block> OnMatched;
        private BlockData BlockData;
        private List<Node> Nodes = new();
        private List<Node> _tempNodes = new();
        private List<BlockMesh> _blockMeshes = new();

        [SerializeField] private GameObject _blockMeshPrefab;
        

        private HighlightEffect _highlightEffect;

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
        
        private bool IsDiagonal(Vector3 dir)
        {
            dir.y = 0; // sadece yatay düzlemde kontrol
            dir.Normalize();

            // Sağ, sol, ileri, geri yönleri
            Vector3[] cardinalDirs = {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right
            };

            foreach (var cardinal in cardinalDirs)
            {
                float angle = Vector3.Angle(dir, cardinal);
                if (angle < 10f) // yaklaşık düz yön
                    return false;
            }

            return true; // düz değilse çaprazdır
        }
        
        private Vector3 IsCollisionDetectedAtPoint(Vector3 point)
        {
            var newPos = transform.position + point * 1f;
            // foreach (Transform child in transform)
            // {
            //     Vector3 start = child.position;
            //     Vector3 direction = (point + child.localPosition) - start;
            //     
            //
            //     direction.Normalize();
            //     float distance = IsDiagonal(direction) ? 1f : .75f; 
            //
            //     // Çizgi çiz debug için
            //     Debug.DrawRay(start, direction.normalized * distance, Color.red, 1f);
            //
            //     if (Physics.Raycast(start, direction, out RaycastHit hit, distance))
            //     {
            //         if (hit.transform.IsChildOf(transform))
            //             continue;
            //     
            //         Debug.Log($"Collision detected! {child.name} hit {hit.transform.name}");
            //
            //         float allowedFactor = hit.distance / distance;
            //
            //         newPos = transform.position + point * allowedFactor;
            //     }
            //     
            // }
            
            return newPos;
        }
        
        public void StartDrag()
        {
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
            var p = GetPosition(pos, meshPos);
            var nextPos = (p - transform.position);
            var check = IsCollisionDetectedAtPoint(nextPos);
            check.y = .1f;
            transform.position = check;
        }

        #region DROP

        public void Drop(Node node)
        {
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
