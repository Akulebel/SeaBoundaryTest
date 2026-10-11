using UnityEngine;

namespace SeaBoundary
{
    // Adds angle-dependent sail push after steering and before the boundary check.
    [DefaultExecutionOrder(100)]
    public sealed class FixedSailWind : MonoBehaviour
    {
        [SerializeField] private Transform ship;
        [SerializeField] private WindState wind;
        [SerializeField, Min(0f)] private float maxAlongWindSpeed = 3f;
        [SerializeField, Min(0f)] private float sideSlipSpeed = 0.35f;
        private float lastHeading;

        public Vector3 WindVelocity
        {
            get
            {
                if (ship == null || wind == null) return Vector3.zero;
                float heading = WindMath.TryHeading(ship.forward, out float current) ? current : lastHeading;
                return WindMath.FixedSailVelocity(heading, wind.Azimuth, wind.Strength,
                    maxAlongWindSpeed, sideSlipSpeed);
            }
        }

        private void OnEnable()
        {
            if (ship == null || wind == null)
            {
                Debug.LogError("FixedSailWind: assign ship and wind.", this);
                enabled = false;
                return;
            }
            if (WindMath.TryHeading(ship.forward, out float heading)) lastHeading = heading;
        }

        private void Update()
        {
            if (WindMath.TryHeading(ship.forward, out float heading)) lastHeading = heading;
            ship.position += WindVelocity * Time.deltaTime;
        }
    }
}
