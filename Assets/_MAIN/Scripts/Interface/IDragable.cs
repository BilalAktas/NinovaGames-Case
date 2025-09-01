using UnityEngine;

namespace NinovaGamesCase.Core
{
    public interface IDragable
    {
        public void StartDrag();
        public void Dragging(Vector3 pos);
        public void Drop(Node node, bool forced);
        public Vector3 GetPosition();
        public Block GetBlock();
    }   
}
