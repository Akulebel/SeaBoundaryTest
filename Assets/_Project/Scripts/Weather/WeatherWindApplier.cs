using UnityEngine;

namespace SeaBoundary
{
    public sealed class WeatherWindApplier : MonoBehaviour
    {
        [SerializeField] private WeatherPresetResolver resolver;
        [SerializeField] private WindState wind;

        private void OnEnable()
        {
            if (resolver == null || wind == null)
            {
                Debug.LogError("WeatherWindApplier: assign resolver and wind.", this);
                enabled = false;
                return;
            }
            resolver.PresetApplied += Apply;
            Apply(resolver.State, resolver.ActivePreset);
        }

        private void OnDisable()
        {
            if (resolver != null) resolver.PresetApplied -= Apply;
            if (wind != null) wind.SetMultiplier(1f);
        }

        private void Apply(WeatherState state, WeatherPreset preset)
        {
            wind.SetMultiplier(preset != null ? preset.WindMultiplier : 1f);
        }
    }
}
