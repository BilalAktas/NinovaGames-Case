using System.Linq;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    public class FreeBlock : Block
    {
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
        /// Handles dragging of blocks across the grid.
        /// </summary>
        public override void Dragging(Vector3 inputPos, Transform grabbedMesh)
        {
            if (!_active) return;
            
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
                transform.position = Vector3.MoveTowards(transform.position, targetPos.Value, 10f * Time.deltaTime);

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