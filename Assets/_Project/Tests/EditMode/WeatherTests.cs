using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SeaBoundary.Tests
{
    public sealed class WeatherTests
    {
        [Test]
        public void WindMultiplierUsesBaseStrengthWithoutCompounding()
        {
            var go = new GameObject("wind test");
            try
            {
                var wind = go.AddComponent<WindState>();
                Set(wind, "strength", 0.6f);
                wind.SetMultiplier(1.6f);
                Assert.That(wind.Strength, Is.EqualTo(0.96f).Within(0.0001f));
                wind.SetMultiplier(0.8f);
                Assert.That(wind.Strength, Is.EqualTo(0.48f).Within(0.0001f));
                wind.SetMultiplier(3f);
                Assert.That(wind.Strength, Is.EqualTo(1f));
                wind.SetMultiplier(1f);
                Assert.That(wind.Strength, Is.EqualTo(0.6f).Within(0.0001f));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void PresetExposesValidatedValues()
        {
            var preset = ScriptableObject.CreateInstance<WeatherPreset>();
            try
            {
                Set(preset, "intensity", 2f);
                Set(preset, "fogDensity", -1f);
                Set(preset, "windMultiplier", -2f);
                Assert.That(preset.Intensity, Is.EqualTo(1f));
                Assert.That(preset.FogDensity, Is.EqualTo(0f));
                Assert.That(preset.WindMultiplier, Is.EqualTo(0f));
            }
            finally { Object.DestroyImmediate(preset); }
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        }
    }
}
