using UnityEngine;

    public abstract class Curve : MonoBehaviour
    {

        [SerializeField] protected GameObject controlVertexPrefab;

        protected LineRenderer lineRenderer;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            
            AddNodeAtPosition(transform.position);
        }

        public abstract void AddNodeAtPosition(Vector2 position);

        public abstract void UpdateNodeAtIndex(int index, Vector2 position);

        public abstract void UpdateLastNode(Vector2 position);

    }