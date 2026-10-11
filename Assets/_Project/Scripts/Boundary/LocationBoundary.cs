using UnityEngine;

namespace SeaBoundary
{
    [ExecuteAlways, RequireComponent(typeof(LineRenderer))]
    public sealed class LocationBoundary : MonoBehaviour
    {
        [SerializeField] private Vector2 halfExtents = new Vector2(12f, 9f);
        [SerializeField, Min(0f)] private float lineHeight = 0.15f;
        private LineRenderer line;
        private Vector2 lastExtents;
        private Vector3 lastCenter;

        public bool Contains(Vector3 worldPosition)
        {
            Vector3 center = transform.position;
            Vector2 extent = new Vector2(Mathf.Max(0.01f, halfExtents.x), Mathf.Max(0.01f, halfExtents.y));
            return Mathf.Abs(worldPosition.x - center.x) <= extent.x && Mathf.Abs(worldPosition.z - center.z) <= extent.y;
        }

        private void OnValidate()
        {
            halfExtents.x = Mathf.Max(0.01f, halfExtents.x);
            halfExtents.y = Mathf.Max(0.01f, halfExtents.y);
            lineHeight = Mathf.Max(0f, lineHeight);
            if (isActiveAndEnabled) Redraw();
        }

        public void RefreshVisual() => Redraw();

        private void OnEnable()
        {
            line = GetComponent<LineRenderer>();
            Redraw();
        }

        private void Update()
        {
            if (lastCenter != transform.position || lastExtents != halfExtents) Redraw();
        }

        private void Redraw()
        {
            if (line == null) line = GetComponent<LineRenderer>();
            lastCenter = transform.position;
            lastExtents = halfExtents;
            float x = Mathf.Max(0.01f, halfExtents.x);
            float z = Mathf.Max(0.01f, halfExtents.y);
            Vector3 c = transform.position + Vector3.up * lineHeight;
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = 4;
            line.SetPosition(0, c + new Vector3(-x, 0f, -z));
            line.SetPosition(1, c + new Vector3(-x, 0f, z));
            line.SetPosition(2, c + new Vector3(x, 0f, z));
            line.SetPosition(3, c + new Vector3(x, 0f, -z));
        }
    }
}
