using System;
using System.Linq;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    public enum LimitedDir {None, Horizontal, Vertical}
    
    public class LimitedBlock : Block
    {
        [SerializeField] private GameObject _horizontalArrowPrefab;
        [SerializeField] private GameObject _verticalArrowPrefab;

        private void Start()
        {
            SetDirectionArrowIcon();
        }

        private void SetDirectionArrowIcon()
        {
            var arrowPrefab = BlockData.LimitedDir == LimitedDir.Horizontal
                ? _horizontalArrowPrefab
                : _verticalArrowPrefab;

            var arrowClone = Instantiate(arrowPrefab, transform);
            arrowClone.transform.localPosition = BlockData.LimitedDir == LimitedDir.Horizontal
                ? new Vector3(((_blockMeshes.Count) / 2f) - .5f, .9f, 0)
                : new Vector3(0, .9f, ((_blockMeshes.Count) / 2f) - .5f);


            var scaleFix = _blockMeshes.Count == 1 ? .15f : .3f;
            arrowClone.transform.localScale = BlockData.LimitedDir == LimitedDir.Horizontal
                ? new Vector3(.15f * (_blockMeshes.Count), scaleFix)
                : new Vector3(scaleFix, .15f * (_blockMeshes.Count));

        }

        /// <summary>
        /// Prepares the block for dragging by saving its nodes and enabling outline.
        /// </summary>
        public override void StartDrag()
        {
            if (!_active) return;

            OutlineShader(true);
            ResetNodes(save: true);
        }

        /// <summary>
        /// Updates the block’s position while being dragged across the grid.
        /// </summary>
        public override void Dragging(Vector3 inputPos, Transform grabbedMesh)
        {
            if (!_active) return;

            inputPos = BlockData.LimitedDir switch
            {
                LimitedDir.Horizontal => new Vector3(inputPos.x, inputPos.y, transform.position.z),
                LimitedDir.Vertical => new Vector3(transform.position.x, inputPos.y, inputPos.z),
                _ => inputPos
            };
            
            var grabbedNode = Grid.Instance.GetSnappedNode(inputPos)
                              ?? Grid.Instance.GetClosestAvailableNode(inputPos);

            if (grabbedNode == null)
            {
                SnapToCurrentNode();
                return;
            }
            
            var grabbedMeshOffsetWorld = transform.rotation * Vector3.Scale(grabbedMesh.localPosition, transform.lossyScale);
            
            var desiredPosition = grabbedNode.WorldPosition - grabbedMeshOffsetWorld;
            
            var targetPos = FindClosestValidPosition(grabbedNode, grabbedMesh, desiredPosition);

            if (targetPos.HasValue)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos.Value, _MOVE_SPEED * Time.deltaTime);

                var p = transform.position;
                p.y = -0.25f;
                transform.position = p;
            }
            else
            {
                SnapToCurrentNode();
            }

            CheckDoorMatch(snapped: false);
        }

        /// <summary>
        /// Attempts to drop the block onto the given node. 
        /// If invalid, resets to its temporary saved nodes.
        /// </summary>
        public override void Drop(Node node, bool forced)
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
    }
}