using UnityEngine;
using UnityEngine.UI;

namespace SeaBoundary
{
    public sealed class WindPresenter : MonoBehaviour
    {
        [SerializeField] private WindState wind;
        [SerializeField] private Transform ship;
        [SerializeField] private Text valuesText;
        [SerializeField] private RectTransform arrow;
        private float lastHeading;
        private float lastAzimuth;
        private float lastStrength;
        private bool dirty = true;

        private void OnEnable()
        {
            if (wind == null || ship == null || valuesText == null || arrow == null)
            {
                Debug.LogError("WindPresenter: assign wind, ship, text and arrow.", this);
                enabled = false;
                return;
            }
            wind.Changed += MarkDirty;
            dirty = true;
            Refresh();
        }

        private void OnDisable()
        {
            if (wind != null) wind.Changed -= MarkDirty;
        }

        private void MarkDirty() => dirty = true;

        private void LateUpdate()
        {
            if (WindMath.TryHeading(ship.forward, out float heading)) lastHeading = heading;
            if (!Mathf.Approximately(lastHeading, previousHeading)) dirty = true;
            if (!Mathf.Approximately(lastAzimuth, wind.Azimuth) || !Mathf.Approximately(lastStrength, wind.Strength)) dirty = true;
            if (dirty) Refresh();
        }

        private float previousHeading;
        private void Refresh()
        {
            lastAzimuth = wind.Azimuth;
            lastStrength = wind.Strength;
            previousHeading = lastHeading;
            float relative = WindMath.RelativeAngle(lastHeading, lastAzimuth);
            valuesText.text = $"Ветер: {lastAzimuth:0}° (куда дует)\nСила: {lastStrength:0.00}\nОт носа: {relative:+0;-0;0}°";
            arrow.localRotation = Quaternion.Euler(0f, 0f, -lastAzimuth);
            dirty = false;
        }
    }
}
