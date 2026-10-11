using UnityEngine;

namespace SeaBoundary
{
    public static class WindMath
    {
        public static float NormalizeAzimuth(float degrees) => Mathf.Repeat(degrees, 360f);

        public static Vector3 Direction(float azimuth)
        {
            float radians = NormalizeAzimuth(azimuth) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        }

        public static bool TryHeading(Vector3 forward, out float heading)
        {
            Vector2 flat = new Vector2(forward.x, forward.z);
            if (flat.sqrMagnitude < 0.000001f)
            {
                heading = 0f;
                return false;
            }
            heading = NormalizeAzimuth(Mathf.Atan2(flat.x, flat.y) * Mathf.Rad2Deg);
            return true;
        }

        public static float RelativeAngle(float shipHeading, float windAzimuth)
        {
            float angle = Mathf.DeltaAngle(NormalizeAzimuth(shipHeading), NormalizeAzimuth(windAzimuth));
            return Mathf.Approximately(angle, -180f) ? 180f : angle;
        }

        // Fixed transverse sail: the projected area and the forward force both
        // diminish as the wind approaches the ship's beam.
        public static Vector3 FixedSailVelocity(float shipHeading, float windAzimuth,
            float strength, float maxAlongWindSpeed, float sideSlipSpeed)
        {
            Vector3 forward = Direction(shipHeading);
            Vector3 windDirection = Direction(windAzimuth);
            float alignment = Vector3.Dot(forward, windDirection);
            Vector3 sideways = windDirection - forward * alignment;
            float along = alignment * Mathf.Abs(alignment) * Mathf.Max(0f, maxAlongWindSpeed);
            return Mathf.Clamp01(strength) *
                   (forward * along + sideways * Mathf.Max(0f, sideSlipSpeed));
        }
    }
}
