using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HighlightPlus;
using NinovaGamesCase.Managers;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    /// <summary>
    /// Defines the available block colors for matching.
    /// </summary>
    public enum MatchColor
    {
        Blue,
        Green,
        Red,
        Yellow
    }

    /// <summary>
    /// Holds a match color and its associated material.
    /// </summary>
    [Serializable]
    public struct ColorData
    {
        public MatchColor MatchColor;
        public Material ColorMaterial;
    }

    /// <summary>
    /// Represents the data used to initialize a block, 
    /// including its color information.
    /// </summary>
    [Serializable]
    public struct BlockData
    {
        public ColorData ColorData;
        public LimitedDir LimitedDir;
    }

    /// <summary>
    /// Represents a block on the grid that can be dragged, dropped,
    /// matched with doors, and highlighted. 
    /// Handles all interactions with nodes and doors.
    /// </summary>
    public abstract class Block : MonoBehaviour
    {
        public static event Action<Vector2, bool> OnForcedDrop;
        public static Action OnMoved;
        public static event Action OnMoveFailed;
        public static event Action<Door, Block> OnMatched;

        [SerializeField] private GameObject _blockMeshPrefab;

        public BlockData BlockData { get; private set; }
        protected List<Node> _nodes = new();
        private List<Node> _tempNodes = new();
        protected List<BlockMesh> _blockMeshes = new();
        private HighlightEffect _highlightEffect;

        protected bool _active = true;
        
        protected const float _MOVE_SPEED = 10f;

        private void Awake()
        {
            _highlightEffect = GetComponent<HighlightEffect>();
        }

        /// <summary>
        /// Initializes the block with level-specific data,
        /// spawns block meshes, assigns nodes, and sets colors.
        /// </summary>
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

        /// <summary>
        /// Handles block removal when the vacuum power-up is triggered.
        /// Shrinks and moves the block toward the target position.
        /// </summary>
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

        #region DRAG
        
        public abstract void StartDrag();
        
        public abstract void Dragging(Vector3 inputPos, Transform grabbedMesh);
        
        public abstract void Drop(Node node, bool forced);

        /// <summary>
        /// Finds the closest valid position near a grabbed node for placement.
        /// </summary>
        protected Vector3? FindClosestValidPosition(Node grabbedNode, Transform grabbedMesh, Vector3 desiredPos)
        {
            var config = LevelManager.CurrentConfig;
            var nodeSize = config.NodeSize;
            var meshOffset = transform.rotation * Vector3.Scale(grabbedMesh.localPosition, transform.lossyScale);
            var basePos = grabbedNode.WorldPosition - meshOffset;

            const int maxRange = 2;
            Vector3? bestPos = null;
            var bestDist = float.MaxValue;

            for (var r = 0; r <= maxRange; r++)
            {
                for (var dx = -r; dx <= r; dx++)
                for (var dz = -r; dz <= r; dz++)
                {
                    var testPos = basePos + new Vector3(dx * nodeSize.x, 0f, dz * nodeSize.y);
                    if (!CanPlaceAllMeshesAt(testPos)) continue;

                    var testWorldPos = testPos + meshOffset;
                    var dist = Vector3.Distance(testWorldPos, grabbedNode.WorldPosition);

                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestPos = testPos;
                    }
                }

                if (bestPos.HasValue) return bestPos;
            }

            return null;
        }
        
        /// <summary>
        /// Checks if all block meshes can be placed at the given parent position.
        /// Rotation & scale-aware.
        /// </summary>
        private bool CanPlaceAllMeshesAt(Vector3 parentPos)
        {
            foreach (var mesh in _blockMeshes)
            {
                var offset = transform.rotation * Vector3.Scale(mesh.transform.localPosition, transform.lossyScale);
                var worldPos = parentPos + offset;

                var node = Grid.Instance.GetSnappedNode(worldPos);
                if (node == null || node.Block || mesh.CheckObstacle(worldPos))
                    return false;
            }

            return true;
        }
        
        #endregion
        
     
        /// <summary>
        /// Snaps the block back to its currently occupied node.
        /// </summary>
        protected void SnapToCurrentNode()
        {
            var node = Grid.Instance.GetSnappedNode(transform.position);
            if (node == null) return;

            var offset = _blockMeshes.Count > 0
                ? transform.rotation * Vector3.Scale(_blockMeshes[0].transform.localPosition, transform.lossyScale)
                : Vector3.zero;

            transform.position = node.WorldPosition - offset;
            transform.position = new Vector3(transform.position.x, -0.25f, transform.position.z);
        }

        /// <summary>
        /// Clears the current nodes and optionally saves them into a temp list.
        /// </summary>
        protected void ResetNodes(bool save)
        {
            if (save) _tempNodes = _nodes.ToList();
            else _tempNodes.Clear();

            foreach (var node in _nodes)
                node.SetBlock(null);

            _nodes.Clear();
        }

        /// <summary>
        /// Restores nodes from the saved temp list when a move fails.
        /// </summary>
        protected void ResetToTempNodes()
        {
            _nodes = _tempNodes;
            foreach (var node in _nodes)
                node.SetBlock(this);

            transform.position = _nodes.First().WorldPosition;
            OnMoveFailed?.Invoke();
        }

        /// <summary>
        /// Validates whether the block can be placed starting at the given pivot node.
        /// Returns the list of valid nodes if successful.
        /// </summary>
        protected bool TryGetValidNodes(Node pivotNode, out List<Node> result)
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

        /// <summary>
        /// Checks whether the block matches with a door,
        /// and triggers forced drop or match event accordingly.
        /// </summary>
        protected void CheckDoorMatch(bool snapped)
        {
            var matchedDoor = FindMatchingDoor();
            if (matchedDoor == null) return;

            var lines = GetUniqueLines(matchedDoor.LevelDoorData.Direction, snapped);
            if (lines.Count > matchedDoor.LevelDoorData.Length) return;
            
            var allLinesMatch = lines.All(line =>
                matchedDoor.CurrentNodes.Any(node =>
                    (matchedDoor.LevelDoorData.Direction is LevelDoorDirection.Up or LevelDoorDirection.Down)
                        ? node.WorldPosition.x == line
                        : node.WorldPosition.z == line));

            if (!allLinesMatch) return;

            if (_active && !snapped)
                OnForcedDrop?.Invoke(Input.mousePosition, true);
            else
            {
                ResetNodes(false);
                OnMatched?.Invoke(matchedDoor, this);
            }
        }

        /// <summary>
        /// Finds a door matching the block’s color under its meshes.
        /// </summary>
        private Door FindMatchingDoor()
        {
            return _blockMeshes
                .Select(mesh => mesh.CheckDoor())
                .FirstOrDefault(door =>
                    door != null && door.LevelDoorData.ColorData.MatchColor == BlockData.ColorData.MatchColor);
        }

        /// <summary>
        /// Returns unique line keys (x or z coordinates) 
        /// based on door direction and snap state.
        /// </summary>
        private HashSet<float> GetUniqueLines(LevelDoorDirection direction, bool snapped)
        {
            var lines = new HashSet<float>();

            if (!snapped)
            {
                var pivot = Grid.Instance.GetSnappedNode(transform.position);
                lines.Add(GetLineKey(pivot.WorldPosition, direction));

                foreach (var child in _blockMeshes)
                    lines.Add(GetLineKey(pivot.WorldPosition + child.transform.localPosition, direction));
            }
            else
            {
                foreach (var child in _blockMeshes)
                    lines.Add(GetLineKey(child.transform.position, direction));
            }

            return lines;
        }

        /// <summary>
        /// Computes a line key (x or z) from world position depending on door direction.
        /// </summary>
        private float GetLineKey(Vector3 pos, LevelDoorDirection dir)
        {
            var s = (dir is LevelDoorDirection.Up or LevelDoorDirection.Down)
                ? pos.x
                : pos.z;
            
            return s;
        }

        /// <summary>
        /// Toggles the block’s outline highlight effect.
        /// </summary>
        protected void OutlineShader(bool state)
        {
            if (_highlightEffect != null)
                _highlightEffect.highlighted = state;
        }

        /// <summary>
        /// Handles block movement animation when matched with a door.
        /// </summary>
        public void OnMatchedMove(Vector3 pos)
        {
            Grid.Instance.AllSpawnedBlocks.Remove(this);
            pos.y = transform.position.y;
            transform.DOMove(pos, 2f).SetEase(Ease.Linear);
            //transform.DOScale(Vector3.zero, 2f).SetEase(Ease.Linear);
        }
    }
}