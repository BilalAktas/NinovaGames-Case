using UnityEngine;


namespace NinovaGamesCase.Core
{
    /// <summary>
    /// Handles the mesh and drag interactions for a block.
    /// Provides collision checks with doors and obstacles, 
    /// and manages visual representation (material/color).
    /// </summary>
    public class BlockMesh : MonoBehaviour, IDragable
    {
        private Block _block;
        private MeshRenderer _render;
        [SerializeField] private LayerMask _doorLayer;
        private BoxCollider _collider;
        
        
        private void Awake()
        {
            _render = GetComponent<MeshRenderer>();
            _collider = GetComponent<BoxCollider>();

        }

        private void Start()
        {
            _block = GetComponentInParent<Block>();
        }

        #region IDRAGABLE
        
        /// <summary>
        /// Gets the current world position of the block (based on its parent transform).
        /// </summary>
        public Vector3 GetPosition() => transform.parent.position;
        
        /// <summary>
        /// Notifies the parent block that a drag operation has started.
        /// </summary>
        public void StartDrag() => _block.StartDrag();
        
        /// <summary>
        /// Updates the block's position while dragging.
        /// </summary>
        /// <param name="pos">The new world position.</param>
        public void Dragging(Vector3 pos) => _block.Dragging(pos, transform);
        
        /// <summary>
        /// Drops the block on the given node.
        /// </summary>
        /// <param name="node">The target node.</param>
        /// <param name="forced">If true, forces placement even if invalid.</param>
        public void Drop(Node node, bool forced) => _block.Drop(node, forced);

        /// <summary>
        /// Gets the parent block associated with this mesh.
        /// </summary>
        public Block GetBlock() => _block;

        #endregion

        /// <summary>
        /// Checks if the block mesh is colliding with a door nearby.
        /// Ignores collisions with its own parent.
        /// </summary>
        /// <returns>The Door component if found, otherwise null.</returns>
        public Door CheckDoor()
        {
            var colliders = Physics.OverlapSphere(transform.position, .55f, _doorLayer);
            foreach (var collider in colliders)
            {
                if(collider.transform.IsChildOf(transform.parent))
                    continue;

                return collider.GetComponentInParent<Door>();
            }

            return null;
        }
        
        /// <summary>
        /// Checks if there is an obstacle between the block and the target position.
        /// Uses a SphereCast to detect collisions.
        /// </summary>
        /// <param name="pos">The target position to check.</param>
        /// <returns>True if an obstacle is detected, false otherwise.</returns>
        public bool CheckObstacle(Vector3 pos)
        {
            var newPos = pos;
            newPos.y = transform.parent.position.y;
            
            var dir = (newPos - transform.position).normalized;
            
            var halfExtents = Vector3.Scale(new Vector3(_collider.size.x * .3f, 2, _collider.size.z * .3f), transform.lossyScale);
            
            var origin = transform.position;
            
            var castDistance = Vector3.Distance(origin, newPos);
            
             RaycastHit hit;
            
            var radius = Mathf.Min(halfExtents.x, halfExtents.z);
            var hasHit = Physics.SphereCast(
                origin,
                radius,
                dir,
                out hit,
                castDistance
            );  

            if (hasHit && !hit.transform.IsChildOf(transform.parent))
                return true;

            return false;
        }
        
        /// <summary>
        /// Sets the material (color) of the block mesh.
        /// </summary>
        /// <param name="mat">The material to apply.</param>
        public void SetColor(Material mat) => _render.material = mat;
        
    }   
}
