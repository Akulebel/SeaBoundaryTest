using System;
using UnityEngine;

namespace SeaBoundary
{
    public sealed class WindState : MonoBehaviour
    {
        [SerializeField, Range(0f, 360f)] private float azimuth = 45f;
        [SerializeField, Range(0f, 1f)] private float strength = 0.6f;
        private float multiplier = 1f;
        private float lastAzimuth;
        private float lastBaseStrength;
        private float lastStrength;
        private float lastMultiplier;
        public float Azimuth => WindMath.NormalizeAzimuth(azimuth);
        public float BaseStrength => Mathf.Clamp01(strength);
        public float WindMultiplier => multiplier;
        public float Strength => Mathf.Clamp01(BaseStrength * multiplier);
        public event Action Changed;

        private void OnEnable()
        {
            lastAzimuth = Azimuth;
            lastBaseStrength = BaseStrength;
            lastStrength = Strength;
            lastMultiplier = multiplier;
        }

        private void OnValidate()
        {
            azimuth = Mathf.Clamp(azimuth, 0f, 360f);
            strength = Mathf.Clamp01(strength);
        }

        public void SetMultiplier(float value)
        {
            float safe = float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Max(0f, value);
            if (Mathf.Approximately(multiplier, safe)) return;
            multiplier = safe;
            NotifyIfChanged();
        }

        private void Update() => NotifyIfChanged();

        private void NotifyIfChanged()
        {
            if (Mathf.Approximately(lastAzimuth, Azimuth) &&
                Mathf.Approximately(lastBaseStrength, BaseStrength) &&
                Mathf.Approximately(lastStrength, Strength) &&
                Mathf.Approximately(lastMultiplier, multiplier)) return;
            lastAzimuth = Azimuth;
            lastBaseStrength = BaseStrength;
            lastStrength = Strength;
            lastMultiplier = multiplier;
            Changed?.Invoke();
        }
    }
}
