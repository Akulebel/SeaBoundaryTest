using UnityEngine;

namespace SeaBoundary
{
    public sealed class WeatherEnvironmentApplier : MonoBehaviour
    {
        [SerializeField] private WeatherPresetResolver resolver;
        [SerializeField] private Light sun;
        private bool originalFog;
        private float originalFogDensity;
        private FogMode originalFogMode;
        private float originalSunIntensity;
        private bool captured;

        private void OnEnable()
        {
            if (resolver == null || sun == null)
            {
                Debug.LogError("WeatherEnvironmentApplier: assign resolver and sun.", this);
                enabled = false;
                return;
            }
            originalFog = RenderSettings.fog;
            originalFogDensity = RenderSettings.fogDensity;
            originalFogMode = RenderSettings.fogMode;
            originalSunIntensity = sun.intensity;
            captured = true;
            resolver.PresetApplied += Apply;
            Apply(resolver.State, resolver.ActivePreset);
        }

        private void OnDisable()
        {
            if (resolver != null) resolver.PresetApplied -= Apply;
            Restore();
        }

        private void Apply(WeatherState state, WeatherPreset preset)
        {
            if (preset == null)
            {
                Restore();
                return;
            }
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = preset.FogDensity;
            RenderSettings.fog = preset.FogDensity > 0f;
            sun.intensity = originalSunIntensity * (1f - 0.45f * preset.Intensity);
        }

        private void Restore()
        {
            if (!captured) return;
            RenderSettings.fog = originalFog;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fogMode = originalFogMode;
            if (sun != null) sun.intensity = originalSunIntensity;
        }
    }
}
