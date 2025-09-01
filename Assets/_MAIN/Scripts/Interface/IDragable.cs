using UnityEngine;

namespace NinovaGamesCase.Core
{
    /// <summary>
    /// Interface for objects that can be dragged and dropped within the grid.
    /// </summary>
    public interface IDragable
    {
        /// <summary>
        /// Called when the dragging action starts.
        /// </summary>
        public void StartDrag();
        
        /// <summary>
        /// Called continuously during dragging, providing the current position.
        /// </summary>
        /// <param name="pos">Current world position of the drag.</param>
        public void Dragging(Vector3 pos);
        
        /// <summary>
        /// Called when the object is dropped onto a node.
        /// </summary>
        /// <param name="node">The target node where the object is dropped.</param>
        /// <param name="forced">Indicates if the drop was forced.</param>
        public void Drop(Node node, bool forced);
        
        /// <summary>
        /// Gets the current world position of the draggable object.
        /// </summary>
        /// <returns>Current world position.</returns>
        public Vector3 GetPosition();
        
        /// <summary>
        /// Gets the Block associated with this draggable object.
        /// </summary>
        /// <returns>The related Block instance.</returns>
        public Block GetBlock();
    }   
}
