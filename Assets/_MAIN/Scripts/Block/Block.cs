using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HighlightPlus;
using NinovaGamesCase.Managers;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    [Serializable]
    public struct ColorData
    {
        public string Color;
        public Material ColorMaterial;
    }

    [Serializable]
    public struct BlockData
    {
        public ColorData ColorData;
    }

    public class Block : MonoBehaviour
    {
        public static event Action<Vector2, bool> OnForcedDrop;
        public static event Action OnMoved;
        public static event Action OnMoveFailed;
        public static event Action<Door, Block> OnMatched;

        [SerializeField] private GameObject _blockMeshPrefab;

        public BlockData BlockData { get; private set; }
        private List<Node> _nodes = new();
        private List<Node> _tempNodes = new();
        private List<BlockMesh> _blockMeshes = new();
        private HighlightEffect _highlightEffect;

        private bool _active = true;

        private void Awake()
        {
            _highlightEffect = GetComponent<HighlightEffect>();
        }

        public void Initialize(LevelBlockData data)
        {
            BlockData = data.BlockData;

            var basePos = data.GridPosition;
            var grid = Grid.Instance.GetGrid();

            var baseNode = grid[basePos.x, basePos.y];
            transform.position = baseNode.WorldPosition;
            _nodes.Add(baseNode);

            foreach (var offset in data.MeshPositions)
            {
                var node = grid[basePos.x + offset.x, basePos.y + offset.y];
                var mesh = Instantiate(_blockMeshPrefab, transform);
                mesh.transform.position = node.WorldPosition;
                mesh.SetActive(true);
                _nodes.Add(node);
            }

            _blockMeshes = GetComponentsInChildren<BlockMesh>().ToList();

            foreach (var node in _nodes)
                node.SetBlock(this);

            foreach (var mesh in _blockMeshes)
                mesh.SetColor(BlockData.ColorData.ColorMaterial);
        }

        public void OnVacuumPowerUp(Vector3 targetPos)
        {
            _active = false;
            
            foreach (var node in _nodes)
                node.SetBlock(null);
            
            _nodes.Clear();
            
            OutlineShader(true);
            
            transform.DOScale(Vector3.zero, .5f).SetEase(Ease.Linear);
            transform.DOMove(targetPos, .5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                OnMatched?.Invoke(null,null);
            });
        }
        
        public void StartDrag()
        {
            if (!_active) return;

            OutlineShader(true);
            ResetNodes(save: true);
        }

        public void Dragging(Vector3 inputPos, Vector3 meshPos)
        {
            if (!_active) return;

            var offset = meshPos - transform.position;
            var targetPos = inputPos - offset;
            targetPos.y = -0.5f;

            var moveDir = targetPos - transform.position;
            var nextPos = transform.position + moveDir * (Time.deltaTime * 10f);

            if (CanMoveTo(nextPos))
                transform.position = nextPos;

            CheckDoorMatch(snapped: false);
        }

        private bool CanMoveTo(Vector3 pos)
        {
            return _blockMeshes.All(mesh => !mesh.CheckObstacle(pos));
        }

        public void Drop(Node node, bool forced)
        {
            if (!_active) return;

            if (node == null || !TryGetValidNodes(node, out var validNodes))
            {
                ResetToTempNodes();
            }
            else
            {
                _nodes = validNodes;
                foreach (var n in _nodes)
                    n.SetBlock(this);

                transform.position = _nodes.First().WorldPosition;
                OnMoved?.Invoke();

                if (forced)
                    _active = false;

                CheckDoorMatch(snapped: true);
            }

            OutlineShader(false);
        }

        private void ResetNodes(bool save)
        {
            if (save) _tempNodes = _nodes.ToList();
            else _tempNodes.Clear();

            foreach (var node in _nodes)
                node.SetBlock(null);

            _nodes.Clear();
        }

        private void ResetToTempNodes()
        {
            _nodes = _tempNodes;
            foreach (var node in _nodes)
                node.SetBlock(this);

            transform.position = _nodes.First().WorldPosition;
            OnMoveFailed?.Invoke();
        }

        private bool TryGetValidNodes(Node pivotNode, out List<Node> result)
        {
            result = new();
            var grid = Grid.Instance.GetGrid();
            var pivotPos = pivotNode.GridPosition;

            var positions = new List<Vector2Int> { pivotPos };
            positions.AddRange(_blockMeshes.Skip(1).Select(mesh =>
            {
                var local = mesh.transform.localPosition;
                return pivotPos + new Vector2Int((int)local.x, (int)local.z);
            }));

            foreach (var pos in positions)
            {
                if (!Grid.Instance.IsNodeInBounds(pos))
                    return false;

                var node = grid[pos.x, pos.y];
                if (node == null || node.Block)
                    return false;

                result.Add(node);
            }

            return true;
        }

        private void CheckDoorMatch(bool snapped)
        {
            var matchedDoor = FindMatchingDoor();
            if (matchedDoor == null) return;

            var lines = GetUniqueLines(matchedDoor.LevelDoorData.Direction, snapped);
            if (lines.Count > matchedDoor.LevelDoorData.Length) return;

            var allLinesMatch = lines.All(line =>
                matchedDoor.CurrentNodes.Any(node =>
                    (matchedDoor.LevelDoorData.Direction is LevelDoorDirection.Up or LevelDoorDirection.Down)
                        ? Mathf.RoundToInt(node.WorldPosition.x) == line
                        : Mathf.RoundToInt(node.WorldPosition.z) == line));

            if (!allLinesMatch) return;

            if (_active && !snapped)
                OnForcedDrop?.Invoke(Input.mousePosition, true);
            else
            {
                ResetNodes(false);
                OnMatched?.Invoke(matchedDoor, this);
            }
        }

        private Door FindMatchingDoor()
        {
            return _blockMeshes
                .Select(mesh => mesh.CheckDoor())
                .FirstOrDefault(door =>
                    door != null && door.LevelDoorData.ColorData.Color == BlockData.ColorData.Color);
        }

        private HashSet<int> GetUniqueLines(LevelDoorDirection direction, bool snapped)
        {
            var lines = new HashSet<int>();

            if (!snapped)
            {
                var pivot = Grid.Instance.GetSnappedNode(transform.position);
                lines.Add(GetLineKey(pivot.WorldPosition, direction));

                foreach (Transform child in transform)
                    lines.Add(GetLineKey(pivot.WorldPosition + child.localPosition, direction));
            }
            else
            {
                foreach (Transform child in transform)
                    lines.Add(GetLineKey(child.position, direction));
            }

            return lines;
        }

        private int GetLineKey(Vector3 pos, LevelDoorDirection dir)
        {
            return (dir is LevelDoorDirection.Up or LevelDoorDirection.Down)
                ? Mathf.RoundToInt(pos.x)
                : Mathf.RoundToInt(pos.z);
        }

        private void OutlineShader(bool state)
        {
            if (_highlightEffect != null)
                _highlightEffect.highlighted = state;
        }

        public void OnMatchedMove(Vector3 pos)
        {
            pos.y = transform.position.y;
            transform.DOMove(pos, 2f).SetEase(Ease.Linear);
        }
    }
}
