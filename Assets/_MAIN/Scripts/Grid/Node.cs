using UnityEngine;

namespace NinovaGamesCase.Core
{
    /// <summary>
    /// Represents a single cell in the grid with its position and associated block.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// The position of the node in grid coordinates.
        /// </summary>
        public Vector2Int GridPosition { get; private set; }
        
        /// <summary>
        /// The position of the node in world space.
        /// </summary>
        public Vector3 WorldPosition { get; private set; }

        /// <summary>
        /// The block occupying this node, if any.
        /// </summary>
        public Block Block { get; private set; }
        
        public Node(Vector2Int gridPosition, Vector3 worldPosition)
        {
            GridPosition = gridPosition;
            WorldPosition = worldPosition;
        }
        
        /// <summary>
        /// Assigns a block to this node.
        /// </summary>
        /// <param name="block">The block to assign.</param>
        public void SetBlock(Block block) => Block = block;
    }    
}

