using System;
using UnityEngine;

namespace SeaBoundary
{
    public enum WeatherState { Clear, Rain, Storm, Fog }

    public sealed class WeatherPresetResolver : MonoBehaviour
    {
        [SerializeField] private WeatherState state;
        [SerializeField] private WeatherPreset clear;
        [SerializeField] private WeatherPreset rain;
        [SerializeField] private WeatherPreset storm;
        [SerializeField] private WeatherPreset fog;

        private WeatherState appliedState;
        private float appliedIntensity;
        private float appliedFogDensity;
        private float appliedWindMultiplier;
        public WeatherState State
        {
            get => state;
            set
            {
                if (!Enum.IsDefined(typeof(WeatherState), value))
                    throw new ArgumentOutOfRangeException(nameof(value));
                if (state == value) return;
                state = value;
                if (isActiveAndEnabled) Apply();
            }
        }

        public WeatherPreset ActivePreset { get; private set; }
        public event Action<WeatherState, WeatherPreset> PresetApplied;

        private void OnEnable()
        {
            if (clear == null || rain == null || storm == null || fog == null)
            {
                Debug.LogError("WeatherPresetResolver: assign all four presets.", this);
                enabled = false;
                return;
            }
            Apply();
        }

        private void OnDisable()
        {
            ActivePreset = null;
            PresetApplied?.Invoke(state, null);
        }

        private void Update()
        {
            WeatherPreset selected = Resolve(state);
            if (selected == null)
            {
                Debug.LogError("WeatherPresetResolver: selected preset is missing.", this);
                enabled = false;
                return;
            }
            if (state != appliedState || selected != ActivePreset ||
                !Mathf.Approximately(selected.Intensity, appliedIntensity) ||
                !Mathf.Approximately(selected.FogDensity, appliedFogDensity) ||
                !Mathf.Approximately(selected.WindMultiplier, appliedWindMultiplier)) Apply();
        }

        private WeatherPreset Resolve(WeatherState value)
        {
            switch (value)
            {
                case WeatherState.Clear: return clear;
                case WeatherState.Rain: return rain;
                case WeatherState.Storm: return storm;
                case WeatherState.Fog: return fog;
                default: return null;
            }
        }

        private void Apply()
        {
            WeatherPreset selected = Resolve(state);
            if (selected == null)
            {
                Debug.LogError("WeatherPresetResolver: selected preset is missing.", this);
                enabled = false;
                return;
            }
            ActivePreset = selected;
            appliedState = state;
            appliedIntensity = selected.Intensity;
            appliedFogDensity = selected.FogDensity;
            appliedWindMultiplier = selected.WindMultiplier;
            PresetApplied?.Invoke(state, selected);
        }
    }
}
