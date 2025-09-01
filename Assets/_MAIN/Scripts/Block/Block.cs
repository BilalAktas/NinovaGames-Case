using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HighlightPlus;
using NinovaGamesCase.Managers;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    public enum MatchColor
    {
        Blue,
        Green,
        Red,
        Yellow
    }

    [Serializable]
    public struct ColorData
    {
        public MatchColor MatchColor;
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

            var _color = transform.GetChild(0).GetComponent<Renderer>().material.GetColor("_Color");
            _highlightEffect.outlineColor = Helpers.Helpers.DarkenColor(_color);
        }

        public void OnVacuumPowerUp(Vector3 targetPos)
        {
            _active = false;

            foreach (var node in _nodes)
                node.SetBlock(null);

            _nodes.Clear();

            OutlineShader(true);

            transform.DOScale(Vector3.zero, .5f).SetEase(Ease.Linear);
            transform.DOMove(targetPos, .5f).SetEase(Ease.Linear).OnComplete(() => { OnMatched?.Invoke(null, null); });
        }

        public void StartDrag()
        {
            if (!_active) return;

            OutlineShader(true);
            ResetNodes(save: true);
        }

        public void Dragging(Vector3 inputPos, Transform grabbedMesh)
        {
            if (!_active) return;

            // Try to get a node under the cursor
            var grabbedNode = Grid.Instance.GetSnappedNode(inputPos) 
                              ?? Grid.Instance.GetClosestAvailableNode(inputPos);

            if (grabbedNode == null)
            {
                SnapToCurrentNode();
                return;
            }

            // Calculate offset based on grabbed mesh
            var offset = grabbedNode.WorldPosition - grabbedMesh.position;
            var desiredPosition = transform.position + offset;

            // Try to find a valid nearby position to place the parent
            var targetPos = FindClosestValidPosition(grabbedNode, grabbedMesh, desiredPosition);

            if (targetPos.HasValue)
            {
                // Smoothly move toward valid position
                transform.position = Vector3.Lerp(transform.position, targetPos.Value, Time.deltaTime * 10f);
            }
            else
            {
                SnapToCurrentNode();
            }

            // Fix Y axis
            var p = transform.position;
            p.y = -0.25f;
            transform.position = p;

            CheckDoorMatch(snapped: false);
        }

      
        private Vector3? FindClosestValidPosition(Node grabbedNode, Transform grabbedMesh, Vector3 desiredPos)
        {
            // Try ranges from smaller to larger
            for (var range = 1; range <= 2; range++)
            {
                Vector3? bestPos = null;
                var closestDist = float.MaxValue;

                for (var dx = -range; dx <= range; dx++)
                {
                    for (var dz = -range; dz <= range; dz++)
                    {
                        var testPos = desiredPos + new Vector3(dx, 0, dz);

                        if (!CanPlaceAllMeshesAt(testPos))
                            continue;

                        var testGrabbedMeshPos = testPos + grabbedMesh.localPosition;
                        var dist = Vector3.Distance(testGrabbedMeshPos, grabbedNode.WorldPosition);

                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            bestPos = testPos;
                        }
                    }
                }

                if (bestPos.HasValue)
                    return bestPos; // Early return if found in this range
            }

            return null;
        }
        
        private void SnapToCurrentNode()
        {
            var node = Grid.Instance.GetSnappedNode(transform.position);
            if (node != null)
                transform.position = node.WorldPosition;
        }

        private bool CanPlaceAllMeshesAt(Vector3 parentPosition)
        {
            foreach (var mesh in _blockMeshes)
            {
                var meshWorldPos = parentPosition + mesh.transform.localPosition;
                
                var node = Grid.Instance.GetSnappedNode(meshWorldPos);
                if (node == null || node.Block)
                    return false;
                
                if (mesh.CheckObstacle(meshWorldPos))
                    return false;
            }

            return true;
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
                    door != null && door.LevelDoorData.ColorData.MatchColor == BlockData.ColorData.MatchColor);
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
            Grid.Instance.AllSpawnedBlocks.Remove(this);
            pos.y = transform.position.y;
            transform.DOMove(pos, 2f).SetEase(Ease.Linear);
            //transform.DOScale(Vector3.zero, 2f).SetEase(Ease.Linear);
        }
    }
}