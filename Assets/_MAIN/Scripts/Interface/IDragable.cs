using UnityEngine;

namespace NinovaGamesCase.Core
{
    public interface IDragable
    {
        public void StartDrag();
        public void Dragging(Vector3 pos);
        public void Drop(Node node);
        public Vector3 GetPosition(Vector3 pos);
    }   
}
