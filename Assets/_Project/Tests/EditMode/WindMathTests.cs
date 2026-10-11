using NUnit.Framework;
using UnityEngine;

namespace SeaBoundary.Tests
{
    public sealed class WindMathTests
    {
        [Test]
        public void AzimuthWrapsAtNorth()
        {
            Assert.That(WindMath.NormalizeAzimuth(360f), Is.EqualTo(0f));
            Assert.That(WindMath.RelativeAngle(359f, 0f), Is.EqualTo(1f).Within(0.001f));
            Assert.That(WindMath.RelativeAngle(0f, 359f), Is.EqualTo(-1f).Within(0.001f));
        }

        [Test]
        public void OppositeDirectionUsesPositive180()
        {
            Assert.That(WindMath.RelativeAngle(0f, 180f), Is.EqualTo(180f));
            Assert.That(WindMath.RelativeAngle(180f, 0f), Is.EqualTo(180f));
        }

        [Test]
        public void HeadingIgnoresPitchAndRejectsVerticalForward()
        {
            Assert.That(WindMath.TryHeading(new Vector3(2f, 100f, 0f), out float heading), Is.True);
            Assert.That(heading, Is.EqualTo(90f));
            Assert.That(WindMath.TryHeading(new Vector3(0.00001f, 1f, 0f), out _), Is.False);
        }

        [Test]
        public void FixedSailRespondsToCourseAndKeepsSmallSideSlip()
        {
            Vector3 tail = WindMath.FixedSailVelocity(0f, 0f, 1f, 3f, 0.35f);
            Vector3 head = WindMath.FixedSailVelocity(0f, 180f, 1f, 3f, 0.35f);
            Vector3 beam = WindMath.FixedSailVelocity(0f, 90f, 1f, 3f, 0.35f);
            Vector3 diagonal = WindMath.FixedSailVelocity(0f, 45f, 1f, 3f, 0.35f);
            Assert.That(tail.z, Is.EqualTo(3f).Within(0.001f));
            Assert.That(head.z, Is.EqualTo(-3f).Within(0.001f));
            Assert.That(beam.z, Is.EqualTo(0f).Within(0.001f));
            Assert.That(beam.x, Is.EqualTo(0.35f).Within(0.001f));
            Assert.That(diagonal.z, Is.EqualTo(1.5f).Within(0.001f));
            Assert.That(WindMath.FixedSailVelocity(0f, 0f, 0f, 3f, 0.35f), Is.EqualTo(Vector3.zero));
        }
    }
}
