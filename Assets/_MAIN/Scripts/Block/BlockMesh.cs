using System;
using UnityEngine;

namespace NinovaGamesCase.Core
{
    public class BlockMesh : MonoBehaviour, IDragable
    {
        private Block _block;
        private MeshRenderer _render;
        [SerializeField] private LayerMask _doorLayer;
        
        private void Awake()
        {
            _render = GetComponent<MeshRenderer>();
        }

        private void Start()
        {
            _block = GetComponentInParent<Block>();
        }

        #region IDRAGABLE
        
        public Vector3 GetPosition(Vector3 pos) => _block.GetPosition(pos, transform.position);
        public void StartDrag() => _block.StartDrag();
        public void Dragging(Vector3 pos) => _block.Dragging(pos, transform.position);
        public void Drop(Node node) => _block.Drop(node);

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

        public void SetColor(Material mat) => _render.material = mat;

        // public void OnDrawGizmosSelected()
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawSphere(transform.position, .3f);
        // }
    }   
}
