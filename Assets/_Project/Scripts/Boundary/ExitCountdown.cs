using System;

namespace SeaBoundary
{
    public enum ExitPhase { Inside, GraceDelay, WarningCountdown, Exited }

    public sealed class ExitCountdown
    {
        public const float GraceSeconds = 1f;
        public const float WarningSeconds = 5f;
        public ExitPhase Phase { get; private set; } = ExitPhase.Inside;
        public float Elapsed { get; private set; }
        public bool WarningVisible => Phase == ExitPhase.WarningCountdown;
        public int RemainingSeconds => WarningVisible ? Math.Max(1, (int)Math.Ceiling(GraceSeconds + WarningSeconds - Elapsed)) : 0;

        public void Reset()
        {
            Phase = ExitPhase.Inside;
            Elapsed = 0f;
        }

        // Returns true exactly once per uninterrupted excursion.
        public bool Tick(bool isOutside, float deltaTime)
        {
            if (!isOutside)
            {
                Reset();
                return false;
            }
            if (Phase == ExitPhase.Exited) return false;
            if (Phase == ExitPhase.Inside) Phase = ExitPhase.GraceDelay;
            Elapsed += Math.Max(0f, deltaTime);
            if (Elapsed >= GraceSeconds + WarningSeconds)
            {
                Phase = ExitPhase.Exited;
                return true;
            }
            if (Elapsed >= GraceSeconds) Phase = ExitPhase.WarningCountdown;
            return false;
        }
    }
}
