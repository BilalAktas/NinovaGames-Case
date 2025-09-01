using System;
using NinovaGamesCase.Managers;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    public class InputDragger : MonoBehaviour
    {
        private Camera _camera;
        private IDragable selectedDragableObject;

        public static event Action<Block> OnPowerUpUsed;

        private void Start()
        {
            _camera = Camera.main;

            Block.OnForcedDrop += HandleDrop;
        }

        private void OnDestroy()
        {
            Block.OnForcedDrop -= HandleDrop;
        }

        private void Update()
        {
            if (GameManager.Instance.GameState == GameState.Idle)
                return;

            if (PowerUpActionManager.Instance.PowerUp == PowerUp.None)
            {
                HandleInput();
            }
            else
            {
                if(Input.GetMouseButtonDown(0)) HandlePowerUpSelection(Input.mousePosition);
            }
       
        }

        private void HandlePowerUpSelection(Vector2 pos)
        {
            if (!PowerUpActionManager.Instance.CanUse)
                return;
            
            var selectedObject = GetObject(pos);
            if (selectedObject && selectedObject.TryGetComponent(out IDragable dragable))
            {
                OnPowerUpUsed?.Invoke(dragable.GetBlock());
            }
        }

        private void HandleInput()
        {
            // if (Input.touchCount <= 0)
            //     return;
            //
            // var touch = Input.GetTouch(0);
            // switch (touch.phase)
            // {
            //     case TouchPhase.Began: HandleStartDrag(touch.position); break;
            //     
            //     case TouchPhase.Moved: 
            //     case TouchPhase.Stationary: HandleDragging(touch.position); break;
            //     
            //     case TouchPhase.Canceled:
            //     case TouchPhase.Ended: HandleDrop(touch.position); break;
            // }
            
            if (Input.GetMouseButtonDown(0))
                HandleStartDrag(Input.mousePosition);
            
            if (Input.GetMouseButton(0))
                HandleDragging(Input.mousePosition);
            
            if (Input.GetMouseButtonUp(0))
                HandleDrop(Input.mousePosition, false);
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

        private void HandleDrop(Vector2 pos, bool forced)
        {
            if (selectedDragableObject == null)
                return;
            
            var node = Grid.Instance.GetSnappedNode(selectedDragableObject.GetPosition());
            
            selectedDragableObject.Drop(node, forced);
            selectedDragableObject = null;
        }

        private Vector3 GetWorldPosition(Vector2 pos)
        {
            var ray = _camera.ScreenPointToRay(pos);
            
            var plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
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