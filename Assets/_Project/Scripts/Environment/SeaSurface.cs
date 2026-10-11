using UnityEngine;

namespace SeaBoundary
{
    public sealed class SeaSurface : MonoBehaviour
    {
        [SerializeField] private Renderer surface;
        [SerializeField] private Vector2 drift = new Vector2(0.004f, 0.008f);

        private static readonly int BaseMapTransform = Shader.PropertyToID("_BaseMap_ST");
        private MaterialPropertyBlock properties;
        private Vector2 tiling;
        private Vector2 offset;

        private void OnEnable()
        {
            if (surface == null) surface = GetComponent<Renderer>();
            if (surface == null || surface.sharedMaterial == null)
            {
                Debug.LogError("SeaSurface: assign a renderer with a material.", this);
                enabled = false;
                return;
            }
            tiling = surface.sharedMaterial.GetTextureScale("_BaseMap");
            properties = new MaterialPropertyBlock();
            offset = Vector2.zero;
        }

        private void Update()
        {
            offset += drift * Time.deltaTime;
            surface.GetPropertyBlock(properties);
            properties.SetVector(BaseMapTransform, new Vector4(tiling.x, tiling.y, offset.x, offset.y));
            surface.SetPropertyBlock(properties);
        }

        private void OnDisable()
        {
            if (surface != null) surface.SetPropertyBlock(null);
        }
    }
}
