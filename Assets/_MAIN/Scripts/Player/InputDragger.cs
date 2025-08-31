using UnityEngine;

namespace NinovaGamesCase.Core
{
    public class InputDragger : MonoBehaviour
    {
        private Camera _camera;
        private IDragable selectedDragableObject;
        [SerializeField] private float _draggerPosZ;

        private void Start() => _camera = Camera.main;

        private void Update()
        {
            if (Input.touchCount <= 0)
                return;
            
            var touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began: HandleStartDrag(touch.position); break;
                
                case TouchPhase.Moved: 
                case TouchPhase.Stationary: HandleDragging(touch.position); break;
                
                case TouchPhase.Canceled:
                case TouchPhase.Ended: HandleDrop(touch.position); break;
            }
            
            // if (Input.GetMouseButtonDown(0))
            //     HandleStartDrag(Input.mousePosition);
            //
            // if (Input.GetMouseButton(0))
            //     HandleDragging(Input.mousePosition);
            //
            // if (Input.GetMouseButtonUp(0))
            //     HandleDrop(Input.mousePosition);
        }

        private void HandleStartDrag(Vector2 pos)
        {
            var selectedObject = GetObject(pos);
            if (selectedObject && selectedObject.TryGetComponent(out IDragable dragable))
            {
                selectedDragableObject = dragable;
                dragable.StartDrag();
            }
        }

        private void HandleDragging(Vector2 pos)
        {
            if (selectedDragableObject == null)
                return;
            
            selectedDragableObject.Dragging(GetWorldPosition(pos));
        }

        private void HandleDrop(Vector2 pos)
        {
            if (selectedDragableObject == null)
                return;
            
            var node = Grid.Instance.GetSnappedNode(selectedDragableObject.GetPosition(GetWorldPosition(pos)));
            
            selectedDragableObject.Drop(node);
            selectedDragableObject = null;
        }

        private Vector3 GetWorldPosition(Vector2 pos)
        {
            var point = new Vector3(pos.x, pos.y, _draggerPosZ);
            return _camera.ScreenToWorldPoint(point);
        }

        private GameObject GetObject(Vector2 pos)
        {
            var ray = _camera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out var hit))
                return hit.transform.gameObject;

            return null;
        }
    }   
}