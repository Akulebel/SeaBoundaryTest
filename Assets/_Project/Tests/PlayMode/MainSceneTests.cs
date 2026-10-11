using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SeaBoundary.Tests
{
    public sealed class MainSceneTests
    {
        private Keyboard testKeyboard;
        private bool createdKeyboard;

        [TearDown]
        public void RestoreTestState()
        {
            Time.timeScale = 1f;
            if (testKeyboard != null && testKeyboard.added)
            {
                InputSystem.QueueStateEvent(testKeyboard, new KeyboardState());
                InputSystem.Update();
                if (createdKeyboard) InputSystem.RemoveDevice(testKeyboard);
            }
            testKeyboard = null;
            createdKeyboard = false;
        }

        [UnityTest]
        public IEnumerator SpeedHudShowsActualSpeedAtDifferentHeadings()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            var ship = Object.FindFirstObjectByType<ShipController>();
            var wind = Object.FindFirstObjectByType<WindState>();
            var speed = Object.FindFirstObjectByType<ShipSpeedPresenter>();
            var speedText = GameObject.Find("ShipSpeed").GetComponent<Text>();
            Assert.That(speedText.text, Does.StartWith("Скорость: "));
            Set(wind, "azimuth", 0f);
            Set(wind, "strength", 1f);
            ship.transform.rotation = Quaternion.identity;
            var keyboard = KeyboardForTest();
            Press(keyboard, Key.W);
            yield return new WaitForSeconds(0.15f);
            Assert.That(speed.CurrentSpeed, Is.InRange(7.5f, 8.5f));
            Assert.That(speedText.text, Does.Contain(speed.CurrentSpeed.ToString("0.0")));
            ship.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            yield return new WaitForSeconds(0.15f);
            Assert.That(speed.CurrentSpeed, Is.InRange(1.5f, 2.5f));
            Release(keyboard);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WeatherStateInInspectorAppliesPresetAndRestoresOnDisable()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            var resolver = Object.FindFirstObjectByType<WeatherPresetResolver>();
            var wind = Object.FindFirstObjectByType<WindState>();
            var sun = GameObject.Find("Sun").GetComponent<Light>();
            var weatherText = GameObject.Find("WeatherValues").GetComponent<Text>();
            Assert.That(resolver.State, Is.EqualTo(WeatherState.Clear));
            Assert.That(wind.Strength, Is.EqualTo(0.6f).Within(0.001f));
            int applied = 0;
            resolver.PresetApplied += (_, preset) => { if (preset != null) applied++; };

            Set(resolver, "state", WeatherState.Rain);
            yield return null;
            Assert.That(resolver.ActivePreset.name, Is.EqualTo("Rain"));
            Assert.That(wind.Strength, Is.EqualTo(0.72f).Within(0.001f));
            Assert.That(weatherText.text, Does.Contain("Rain"));
            Assert.That(RenderSettings.fogDensity, Is.EqualTo(0.012f).Within(0.0001f));

            Set(resolver, "state", WeatherState.Storm);
            yield return null;
            Assert.That(wind.Strength, Is.EqualTo(0.96f).Within(0.001f));
            Assert.That(sun.intensity, Is.LessThan(1f));
            Assert.That(weatherText.text, Does.Contain("Storm"));

            Set(resolver, "state", WeatherState.Fog);
            yield return null;
            Assert.That(wind.Strength, Is.EqualTo(0.48f).Within(0.001f));
            Assert.That(RenderSettings.fogDensity, Is.EqualTo(0.06f).Within(0.0001f));
            Assert.That(weatherText.text, Does.Contain("Fog"));

            Set(resolver, "state", WeatherState.Clear);
            yield return null;
            Assert.That(wind.Strength, Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(RenderSettings.fog, Is.False);
            Assert.That(applied, Is.EqualTo(4));

            resolver.State = WeatherState.Storm;
            Assert.That(wind.Strength, Is.EqualTo(0.96f).Within(0.001f));
            resolver.enabled = false;
            Assert.That(wind.Strength, Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(weatherText.text, Does.Contain("—"));
            resolver.enabled = true;
            Assert.That(wind.Strength, Is.EqualTo(0.96f).Within(0.001f));
            Assert.That(applied, Is.EqualTo(6));
        }

        [UnityTest]
        public IEnumerator ExistingPlayerMoveActionDrivesTranslationAndTurning()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            var ship = Object.FindFirstObjectByType<ShipController>();
            Object.FindFirstObjectByType<FixedSailWind>().enabled = false;
            var keyboard = KeyboardForTest();
            Vector3 initialPosition = ship.transform.position;
            float initialHeading = ship.transform.eulerAngles.y;
            Press(keyboard, Key.W, Key.D);
            yield return new WaitForSeconds(0.15f);
            Release(keyboard);
            yield return null;
            Assert.That(Vector3.Distance(ship.transform.position, initialPosition), Is.GreaterThan(0.05f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(initialHeading, ship.transform.eulerAngles.y)), Is.GreaterThan(1f));
        }

        [UnityTest]
        public IEnumerator WindPushesSidewaysAndChangesForwardSpeed()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            var ship = Object.FindFirstObjectByType<ShipController>();
            var wind = Object.FindFirstObjectByType<WindState>();
            var sailWind = Object.FindFirstObjectByType<FixedSailWind>();
            Assert.NotNull(sailWind);
            ship.enabled = false;
            Set(wind, "azimuth", 90f);
            Set(wind, "strength", 1f);
            ship.transform.position = Vector3.zero;
            yield return new WaitForSeconds(0.3f);
            Assert.That(ship.transform.position.x, Is.GreaterThan(0.05f));
            Assert.That(ship.transform.position.x, Is.LessThan(0.3f));
            Assert.That(Mathf.Abs(ship.transform.position.z), Is.LessThan(0.05f));
            ship.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            ship.transform.position = new Vector3(11.8f, 0f, 0f);
            yield return new WaitForSeconds(0.25f);
            Assert.That(Object.FindFirstObjectByType<LocationExitController>().Phase, Is.EqualTo(ExitPhase.GraceDelay));

            Set(wind, "strength", 0f);
            ship.transform.rotation = Quaternion.identity;
            ship.transform.position = Vector3.zero;
            yield return new WaitForSeconds(0.2f);
            Assert.That(ship.transform.position.magnitude, Is.LessThan(0.01f));

            ship.enabled = true;
            ship.transform.rotation = Quaternion.identity;
            var keyboard = KeyboardForTest();
            Press(keyboard, Key.W);
            yield return null;
            ship.transform.position = Vector3.zero;
            yield return new WaitForSeconds(0.35f);
            float calmDistance = ship.transform.position.z;
            Assert.That(calmDistance, Is.GreaterThan(1f));

            Set(wind, "azimuth", 180f);
            Set(wind, "strength", 1f);
            ship.transform.position = Vector3.zero;
            yield return new WaitForSeconds(0.35f);
            float headwindDistance = ship.transform.position.z;
            Assert.That(headwindDistance, Is.GreaterThan(0f));
            Assert.That(headwindDistance, Is.LessThan(calmDistance * 0.7f));

            Set(wind, "azimuth", 0f);
            ship.transform.position = Vector3.zero;
            yield return new WaitForSeconds(0.35f);
            Assert.That(ship.transform.position.z, Is.GreaterThan(calmDistance * 1.3f));
            Release(keyboard);
            yield return null;
        }

        [UnityTest]
        public IEnumerator WindAndShipHeadingRefreshTheHud()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            var wind = Object.FindFirstObjectByType<WindState>();
            var ship = Object.FindFirstObjectByType<ShipController>();
            var presenter = Object.FindFirstObjectByType<WindPresenter>();
            Assert.NotNull(wind);
            Assert.NotNull(ship);
            Assert.NotNull(presenter);
            var values = GameObject.Find("WindValues").GetComponent<Text>();
            var arrow = GameObject.Find("WindArrow").GetComponent<RectTransform>();
            var compass = GameObject.Find("Compass").GetComponent<RectTransform>();
            var corners = new Vector3[4];
            compass.GetWorldCorners(corners);
            Assert.That(Vector3.Distance(arrow.TransformPoint(arrow.rect.center),
                (corners[0] + corners[2]) * 0.5f), Is.LessThan(0.001f));
            Set(wind, "azimuth", 360f);
            Set(wind, "strength", 0.8f);
            yield return null;
            Assert.That(values.text, Does.Contain("Ветер: 0°"));
            Assert.That(values.text, Does.Contain("Сила: " + 0.8f.ToString("0.00")));
            Assert.That(Mathf.DeltaAngle(arrow.localEulerAngles.z, 0f), Is.EqualTo(0f).Within(0.01f));
            Set(wind, "azimuth", 45f);
            ship.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            yield return null;
            Assert.That(values.text, Does.Contain("От носа: -45°"));
            Assert.That(Mathf.DeltaAngle(arrow.localEulerAngles.z, -45f), Is.EqualTo(0f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator LeavingAndReturningRunsOneCountdownPerExcursion()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            Time.timeScale = 10f;
            var ship = Object.FindFirstObjectByType<ShipController>();
            Object.FindFirstObjectByType<FixedSailWind>().enabled = false;
            var controller = Object.FindFirstObjectByType<LocationExitController>();
            var boundary = Object.FindFirstObjectByType<LocationBoundary>();
            var warning = GameObject.Find("ExitWarning").GetComponent<Text>();
            Assert.That(boundary.Contains(new Vector3(12f, 0f, 0f)), Is.True);
            Assert.That(boundary.Contains(new Vector3(12.01f, 0f, 0f)), Is.False);
            int exits = 0;
            controller.OnLocationExit += () => exits++;
            ship.transform.position = new Vector3(13f, 0.35f, 0f);
            yield return new WaitForSeconds(0.5f);
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.GraceDelay));
            Assert.That(warning.enabled, Is.False);
            ship.transform.position = new Vector3(0f, 0.35f, 0f);
            yield return null;
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.Inside));
            ship.transform.position = new Vector3(13f, 0.35f, 0f);
            yield return new WaitForSeconds(1.5f);
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.WarningCountdown));
            Assert.That(warning.enabled, Is.True);
            ship.transform.position = new Vector3(0f, 0.35f, 0f);
            yield return null;
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.Inside));
            Assert.That(warning.enabled, Is.False);
            ship.transform.position = new Vector3(13f, 0.35f, 0f);
            yield return new WaitForSeconds(5f);
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.WarningCountdown));
            Assert.That(controller.RemainingSeconds, Is.EqualTo(1));
            Assert.That(warning.text, Does.Contain("  1"));
            ship.transform.position = new Vector3(0f, 0.35f, 0f);
            yield return null;
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.Inside));
            Assert.That(warning.enabled, Is.False);
            ship.transform.position = new Vector3(13f, 0.35f, 0f);
            LogAssert.Expect(LogType.Log, "Location exited");
            yield return new WaitForSeconds(6.5f);
            Assert.That(exits, Is.EqualTo(1));
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.Exited));
            Assert.That(warning.enabled, Is.False);
            yield return new WaitForSeconds(1f);
            Assert.That(exits, Is.EqualTo(1));
            ship.transform.position = new Vector3(0f, 0.35f, 0f);
            yield return null;
            ship.transform.position = new Vector3(13f, 0.35f, 0f);
            LogAssert.Expect(LogType.Log, "Location exited");
            yield return new WaitForSeconds(6.5f);
            Assert.That(exits, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator StartingOutsideAndDisablingControllerResetTheCycle()
        {
            SceneManager.LoadScene("MainScene");
            yield return null;
            Time.timeScale = 10f;
            var ship = Object.FindFirstObjectByType<ShipController>();
            Object.FindFirstObjectByType<FixedSailWind>().enabled = false;
            var controller = Object.FindFirstObjectByType<LocationExitController>();
            ship.transform.position = new Vector3(13f, 0.35f, 0f);
            yield return new WaitForSeconds(1.5f);
            Assert.That(controller.WarningVisible, Is.True);
            controller.enabled = false;
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.Inside));
            controller.enabled = true;
            yield return new WaitForSeconds(0.5f);
            Assert.That(controller.WarningVisible, Is.False);
            LogAssert.Expect(LogType.Log, "Location exited");
            yield return new WaitForSeconds(6.5f);
            Assert.That(controller.Phase, Is.EqualTo(ExitPhase.Exited));
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        }

        private Keyboard KeyboardForTest()
        {
            testKeyboard = Keyboard.current;
            if (testKeyboard != null) return testKeyboard;
            testKeyboard = InputSystem.AddDevice<Keyboard>();
            createdKeyboard = true;
            return testKeyboard;
        }

        private static void Press(Keyboard keyboard, params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
        }

        private static void Release(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }
    }
}
