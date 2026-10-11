using System.Reflection;
using SeaBoundary;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SeaBoundary.Editor
{
    public static class MainSceneBuilder
    {
        private const string Root = "Assets/_Project";

        [MenuItem("Sea Boundary/Rebuild Main Scene")]
        public static void Build()
        {
            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Root + "/Prefabs");
            EnsureFolder(Root + "/Materials");
            EnsureFolder(Root + "/Meshes");
            EnsureFolder(Root + "/Presets");
            EnsureFolder(Root + "/Textures");
            EnsureFolder(Root + "/UI");

            var clearPreset = Preset("Clear", 0f, 0f, 1f);
            var rainPreset = Preset("Rain", 0.45f, 0.012f, 1.2f);
            var stormPreset = Preset("Storm", 1f, 0.025f, 1.6f);
            var fogPreset = Preset("Fog", 0.7f, 0.06f, 0.8f);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var interactiveObjects = new GameObject("Interactive objects").transform;
            var nonInteractiveObjects = new GameObject("Non-interactive").transform;
            var waterMat = Material("Water", Color.white, false);
            var hullMat = Material("Hull", new Color(0.38f, 0.19f, 0.11f), false);
            var deckMat = Material("Deck", new Color(0.71f, 0.48f, 0.27f), false);
            var trimMat = Material("ShipTrim", new Color(0.92f, 0.67f, 0.38f), true);
            var sailMat = Material("Sail", new Color(0.96f, 0.91f, 0.73f), true);
            var mastMat = Material("Mast", new Color(0.28f, 0.16f, 0.10f), false);
            var cabinMat = Material("Cabin", new Color(0.86f, 0.68f, 0.42f), false);
            var glassMat = Material("CabinGlass", new Color(0.11f, 0.32f, 0.39f), false);
            var borderMat = Material("Boundary", new Color(1f, 0.75f, 0.18f), true);

            MainSceneVisualsBuilder.BuildWater(waterMat, nonInteractiveObjects);

            var ship = new GameObject("Ship");
            ship.SetActive(false);
            ship.transform.position = new Vector3(0f, 0.35f, 0f);
            MainSceneVisualsBuilder.BuildShip(ship.transform, hullMat, deckMat, trimMat,
                sailMat, mastMat, cabinMat, glassMat);
            var input = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            if (input == null) throw new System.InvalidOperationException("InputSystem_Actions.inputactions missing");
            Set(ship.AddComponent<ShipController>(), "actions", input);
            PrefabUtility.SaveAsPrefabAssetAndConnect(ship, Root + "/Prefabs/Ship.prefab", InteractionMode.AutomatedAction);
            ship.SetActive(true);

            var boundaryObject = new GameObject("LocationBoundary");
            boundaryObject.transform.SetParent(nonInteractiveObjects, false);
            boundaryObject.SetActive(false);
            var line = boundaryObject.AddComponent<LineRenderer>();
            line.sharedMaterial = borderMat;
            line.startWidth = 0.14f;
            line.endWidth = 0.14f;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            var boundary = boundaryObject.AddComponent<LocationBoundary>();
            boundaryObject.SetActive(true);
            boundary.RefreshVisual();

            var windObject = new GameObject("WindState");
            windObject.transform.SetParent(interactiveObjects, false);
            var wind = windObject.AddComponent<WindState>();

            var weatherObject = new GameObject("WeatherPresetResolver");
            weatherObject.transform.SetParent(interactiveObjects, false);
            weatherObject.SetActive(false);
            var weather = weatherObject.AddComponent<WeatherPresetResolver>();
            Set(weather, "clear", clearPreset);
            Set(weather, "rain", rainPreset);
            Set(weather, "storm", stormPreset);
            Set(weather, "fog", fogPreset);
            weatherObject.SetActive(true);

            var weatherWindObject = new GameObject("WeatherWindApplier");
            weatherWindObject.transform.SetParent(nonInteractiveObjects, false);
            weatherWindObject.SetActive(false);
            var weatherWind = weatherWindObject.AddComponent<WeatherWindApplier>();
            Set(weatherWind, "resolver", weather);
            Set(weatherWind, "wind", wind);
            weatherWindObject.SetActive(true);

            var sailWindObject = new GameObject("FixedSailWind");
            sailWindObject.transform.SetParent(nonInteractiveObjects, false);
            sailWindObject.SetActive(false);
            var sailWind = sailWindObject.AddComponent<FixedSailWind>();
            Set(sailWind, "ship", ship.transform);
            Set(sailWind, "wind", wind);
            sailWindObject.SetActive(true);

            var exitObject = new GameObject("LocationExit");
            exitObject.transform.SetParent(nonInteractiveObjects, false);
            exitObject.SetActive(false);
            var exit = exitObject.AddComponent<LocationExitController>();
            Set(exit, "ship", ship.transform);
            Set(exit, "boundary", boundary);
            var logger = exitObject.AddComponent<LocationExitLogger>();
            Set(logger, "controller", exit);
            exitObject.SetActive(true);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 16.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.07f, 0.1f);
            cameraObject.transform.position = new Vector3(0f, 27f, -16f);
            cameraObject.transform.LookAt(new Vector3(0f, 0f, 0f));

            var lightObject = new GameObject("Sun");
            lightObject.transform.SetParent(nonInteractiveObjects, false);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var environmentObject = new GameObject("WeatherEnvironmentApplier");
            environmentObject.transform.SetParent(nonInteractiveObjects, false);
            environmentObject.SetActive(false);
            var environment = environmentObject.AddComponent<WeatherEnvironmentApplier>();
            Set(environment, "resolver", weather);
            Set(environment, "sun", light);
            environmentObject.SetActive(true);

            MainSceneHudBuilder.Build(ship.transform, wind, weather, exit);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, Root + "/Scenes/MainScene.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "/Scenes/MainScene.unity", true) };
            AssetDatabase.SaveAssets();
            Debug.Log("MainScene built and added to Build Settings.");
        }

        private static Material Material(string name, Color color, bool unlit)
        {
            string path = Root + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            string shaderName = unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
            var shader = Shader.Find(shaderName);
            if (shader == null) throw new System.InvalidOperationException("Shader missing: " + shaderName);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else if (material.shader != shader) material.shader = shader;
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static WeatherPreset Preset(string name, float intensity, float fogDensity, float windMultiplier)
        {
            string path = Root + "/Presets/" + name + ".asset";
            var preset = AssetDatabase.LoadAssetAtPath<WeatherPreset>(path);
            if (preset != null) return preset;
            preset = ScriptableObject.CreateInstance<WeatherPreset>();
            AssetDatabase.CreateAsset(preset, path);
            Set(preset, "intensity", intensity);
            Set(preset, "fogDensity", fogDensity);
            Set(preset, "windMultiplier", windMultiplier);
            return preset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            int index = path.LastIndexOf('/');
            string parent = path.Substring(0, index);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(index + 1));
        }

        private static void Set(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new System.MissingFieldException(target.GetType().Name, name);
            field.SetValue(target, value);
            EditorUtility.SetDirty((Object)target);
        }
    }
}
