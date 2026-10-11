using UnityEngine;

namespace SeaBoundary
{
    [CreateAssetMenu(fileName = "WeatherPreset", menuName = "Sea Boundary/Weather Preset")]
    public sealed class WeatherPreset : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float intensity;
        [SerializeField, Min(0f)] private float fogDensity;
        [SerializeField, Min(0f)] private float windMultiplier = 1f;

        public float Intensity => SafeNonNegative(intensity, 0f, 1f);
        public float FogDensity => SafeNonNegative(fogDensity, 0f, float.MaxValue);
        public float WindMultiplier => SafeNonNegative(windMultiplier, 1f, float.MaxValue);

        private void OnValidate()
        {
            intensity = Intensity;
            fogDensity = FogDensity;
            windMultiplier = WindMultiplier;
        }

        private static float SafeNonNegative(float value, float fallback, float maximum)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return fallback;
            return Mathf.Clamp(value, 0f, maximum);
        }
    }
}
