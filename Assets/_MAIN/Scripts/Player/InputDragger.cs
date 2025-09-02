using System;
using NinovaGamesCase.Managers;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    /// <summary>
    /// Handles input for dragging and dropping objects that implement IDragable.
    /// Also manages power-up usage input.
    /// </summary>
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
            if (GameManager.Instance.GameState == GameState.Idle) return;

            if (PowerUpActionManager.Instance.PowerUp != PowerUp.None)
            {
                if (Input.GetMouseButtonDown(0)) 
                    HandlePowerUpSelection(Input.mousePosition);
                return;
            }

            HandleInput();
       
        }

        /// <summary>
        /// Handles the selection of a block when a power-up is active.
        /// </summary>
        /// <param name="pos">Screen position of the input.</param>
        private void HandlePowerUpSelection(Vector2 pos)
        {
            if (!PowerUpActionManager.Instance.CanUse) return;

            if (TryGetDragableAtPosition(pos, out var dragable))
            {
                OnPowerUpUsed?.Invoke(dragable.GetBlock());
            }
        }

        /// <summary>
        /// Handles drag input when no power-up is active.
        /// </summary>
        private void HandleInput()
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
                case TouchPhase.Ended: HandleDrop(touch.position, false); break;
            }
            
            // if (Input.GetMouseButtonDown(0))
            //     HandleStartDrag(Input.mousePosition);
            //
            // if (Input.GetMouseButton(0))
            //     HandleDragging(Input.mousePosition);
            //
            // if (Input.GetMouseButtonUp(0))
            //     HandleDrop(Input.mousePosition, false);
        }

        /// <summary>
        /// Starts dragging the object under the input position.
        /// </summary>
        /// <param name="pos">Screen position of the input.</param>
        private void HandleStartDrag(Vector2 pos)
        {
            if (TryGetDragableAtPosition(pos, out var dragable))
            {
                selectedDragableObject = dragable;
                dragable.StartDrag();
            }
        }
        
        private bool TryGetDragableAtPosition(Vector2 pos, out IDragable dragable)
        {
            dragable = null;
            var selectedObject = GetObject(pos);
            return selectedObject != null && selectedObject.TryGetComponent(out dragable);
        }

        /// <summary>
        /// Updates the position of the dragged object based on input position.
        /// </summary>
        /// <param name="pos">Screen position of the input.</param>
        private void HandleDragging(Vector2 pos)
        {
            if (selectedDragableObject == null)
                return;
            
            selectedDragableObject.Dragging(GetWorldPosition(pos));
        }

        /// <summary>
        /// Drops the dragged object onto the closest snapped node.
        /// </summary>
        /// <param name="pos">Screen position of the input.</param>
        /// <param name="forced">Whether the drop is forced or not.</param>
        private void HandleDrop(Vector2 pos, bool forced)
        {
            if (selectedDragableObject == null)
                return;
            
            var node = Grid.Instance.GetSnappedNode(selectedDragableObject.GetPosition());
            
            selectedDragableObject.Drop(node, forced);
            selectedDragableObject = null;
        }

        /// <summary>
        /// Converts screen position to the nearest node center on the XZ plane.
        /// Works for both odd and even grid sizes.
        /// </summary>
        /// <param name="pos">Screen position (mouse/touch).</param>
        /// <returns>Snapped world position on the grid.</returns>
        private Vector3 GetWorldPosition(Vector2 pos)
        {
            var ray = _camera.ScreenPointToRay(pos);

            var plane = new Plane(Vector3.up, new Vector3(0, -.5f, 0));
            if (plane.Raycast(ray, out var distance))
            {
               return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Returns the GameObject hit by a raycast from the input screen position.
        /// </summary>
        /// <param name="pos">Screen position.</param>
        /// <returns>GameObject hit or null if nothing hit.</returns>
        private GameObject GetObject(Vector2 pos)
        {
            var ray = _camera.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out var hit))
                return hit.transform.gameObject;

            return null;
        }
    }   
}