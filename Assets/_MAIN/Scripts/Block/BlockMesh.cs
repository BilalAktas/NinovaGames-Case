using UnityEngine;


namespace NinovaGamesCase.Core
{
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
        
        public Vector3 GetPosition(Vector3 pos) => transform.parent.position;
        public void StartDrag() => _block.StartDrag();
        public void Dragging(Vector3 pos) => _block.Dragging(pos, transform.position);
        public void Drop(Node node, bool forced) => _block.Drop(node, forced);

        public Block GetBlock() => _block;

        #endregion


        public Door CheckDoor()
        {
            var colliders = Physics.OverlapSphere(transform.position, .5f, _doorLayer);
            foreach (var collider in colliders)
            {
                if(collider.transform.IsChildOf(transform.parent))
                    continue;

                return collider.GetComponentInParent<Door>();
            }

            return null;
        }
        
        public bool CheckObstacle(Vector3 pos)
        {
            var newPos = pos + transform.localPosition;
            newPos.y = transform.parent.position.y;
            
            var dir = (newPos - transform.position).normalized;
            
            var halfExtents = Vector3.Scale(_collider.size * .35f, transform.lossyScale);
            
            var origin = transform.position;
            
            var castDistance = Vector3.Distance(origin, newPos);
            
            RaycastHit hit;
            var hasHit = Physics.BoxCast(
                origin,
                halfExtents,
                dir,
                out hit,
                transform.rotation,
                castDistance
            );

            if (hasHit && !hit.transform.IsChildOf(transform.parent))
            {
                Debug.DrawRay(origin, dir * hit.distance, Color.red);
                
                var margin = 0.01f;
                var stopParentPosition = hit.point - transform.localPosition - dir * margin;
                stopParentPosition.y = transform.parent.position.y;

                return true;
            }

            return false;
        }
        
        public void SetColor(Material mat) => _render.material = mat;
        
    }   
}
