using NUnit.Framework;

namespace SeaBoundary.Tests
{
    public sealed class ExitCountdownTests
    {
        [Test]
        public void GraceDelayCanBeCancelled()
        {
            var timer = new ExitCountdown();
            timer.Tick(true, 0.9f);
            Assert.That(timer.Phase, Is.EqualTo(ExitPhase.GraceDelay));
            timer.Tick(false, 0f);
            Assert.That(timer.Phase, Is.EqualTo(ExitPhase.Inside));
            timer.Tick(true, 0.9f);
            Assert.That(timer.WarningVisible, Is.False);
        }

        [Test]
        public void WarningCanBeCancelledAndRestarted()
        {
            var timer = new ExitCountdown();
            timer.Tick(true, 3f);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(3));
            timer.Tick(false, 0f);
            Assert.That(timer.WarningVisible, Is.False);
            timer.Tick(true, 1f);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(5));
        }

        [Test]
        public void ExitFiresOnceUntilReturnInside()
        {
            var timer = new ExitCountdown();
            Assert.That(timer.Tick(true, 6f), Is.True);
            Assert.That(timer.Tick(true, 10f), Is.False);
            timer.Tick(false, 0f);
            Assert.That(timer.Tick(true, 6f), Is.True);
        }

        [Test]
        public void LargeFrameCarriesAcrossBothPhases()
        {
            var timer = new ExitCountdown();
            timer.Tick(true, 0.5f);
            Assert.That(timer.Tick(true, 5.4f), Is.False);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(1));
            Assert.That(timer.Tick(true, 0.1f), Is.True);
        }
    }
}
