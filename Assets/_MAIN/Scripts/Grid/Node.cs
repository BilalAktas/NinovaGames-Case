using UnityEngine;

namespace NinovaGamesCase.Core
{
    public class Node
    {
        public Vector2Int GridPosition { get; private set; }
        public Vector3 WorldPosition { get; private set; }

        public Block Block { get; private set; }
        
        
        public Node(Vector2Int gridPosition, Vector3 worldPosition)
        {
            GridPosition = gridPosition;
            WorldPosition = worldPosition;
        }

        public void SetBlock(Block block) => Block = block;
    }    
}

