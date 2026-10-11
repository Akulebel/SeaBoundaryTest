using System.IO;
using SeaBoundary;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SeaBoundary.Editor
{
    internal static class MainSceneHudBuilder
    {
        private const string RoundedSpritePath = "Assets/_Project/UI/RoundedPanel.png";
        private static readonly Color Panel = new Color32(7, 28, 43, 238);
        private static readonly Color SoftText = new Color32(166, 204, 213, 255);
        private static readonly Color BrightText = new Color32(246, 250, 248, 255);
        private static readonly Color Aqua = new Color32(91, 218, 204, 255);

        public static void Build(Transform ship, WindState wind, WeatherPresetResolver weather, LocationExitController exit)
        {
            Sprite rounded = RoundedSprite();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var hud = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            hud.SetActive(false);
            hud.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = hud.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Image windPanel = Card("WindPanel", hud.transform, rounded, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(600f, 250f), Panel);
            Stripe("WindAccent", windPanel.transform, rounded, new Vector2(26f, -6f),
                new Vector2(548f, 6f), Aqua);
            Label("WindTitle", windPanel.transform, font, "НАВИГАЦИЯ", 23, SoftText,
                new Vector2(30f, -23f), new Vector2(440f, 32f));
            Text speed = Label("ShipSpeed", windPanel.transform, font, "Скорость: 0,0 ед/с", 45, BrightText,
                new Vector2(30f, -57f), new Vector2(540f, 61f));
            Stripe("WindDivider", windPanel.transform, rounded, new Vector2(30f, -128f),
                new Vector2(540f, 2f), new Color(0.5f, 0.77f, 0.8f, 0.28f));
            Text values = Label("WindValues", windPanel.transform, font, "", 27, SoftText,
                new Vector2(30f, -140f), new Vector2(420f, 105f));
            values.lineSpacing = 1.12f;
            Image compass = Card("Compass", windPanel.transform, rounded, new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-27f, -140f), new Vector2(106f, 106f),
                new Color(0.1f, 0.28f, 0.36f, 0.95f), false);
            Text north = Label("North", compass.transform, font, "N", 21, SoftText,
                new Vector2(0f, -7f), new Vector2(106f, 26f));
            north.alignment = TextAnchor.MiddleCenter;
            Text arrow = Label("WindArrow", compass.transform, font, "▲", 59,
                new Color32(255, 207, 121, 255), Vector2.zero, new Vector2(94f, 70f));
            arrow.alignment = TextAnchor.MiddleCenter;
            arrow.rectTransform.anchorMin = arrow.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            arrow.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            arrow.rectTransform.anchoredPosition = Vector2.zero;

            var windUi = windPanel.gameObject.AddComponent<WindPresenter>();
            Set(windUi, "wind", wind);
            Set(windUi, "ship", ship);
            Set(windUi, "valuesText", values);
            Set(windUi, "arrow", arrow.rectTransform);
            var speedUi = windPanel.gameObject.AddComponent<ShipSpeedPresenter>();
            Set(speedUi, "ship", ship);
            Set(speedUi, "speedText", speed);

            Image weatherPanel = Card("WeatherPanel", hud.transform, rounded, new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(520f, 230f), Panel);
            Image weatherAccent = Stripe("WeatherAccent", weatherPanel.transform, rounded,
                new Vector2(20f, -22f), new Vector2(7f, 184f), new Color32(255, 207, 121, 255));
            Label("WeatherTitle", weatherPanel.transform, font, "УСЛОВИЯ В МОРЕ", 22, SoftText,
                new Vector2(42f, -22f), new Vector2(430f, 30f));
            Text weatherValues = Label("WeatherValues", weatherPanel.transform, font, "", 35, BrightText,
                new Vector2(42f, -58f), new Vector2(440f, 57f));
            Text weatherDetail = Label("WeatherDetail", weatherPanel.transform, font, "", 25, SoftText,
                new Vector2(42f, -124f), new Vector2(440f, 84f));
            weatherDetail.lineSpacing = 1.15f;
            var weatherUi = weatherPanel.gameObject.AddComponent<WeatherPresenter>();
            Set(weatherUi, "resolver", weather);
            Set(weatherUi, "weatherText", weatherValues);
            Set(weatherUi, "detailText", weatherDetail);
            Set(weatherUi, "accent", weatherAccent);

            Image helpPanel = Card("ControlsHint", hud.transform, rounded, new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(520f, 68f),
                new Color(0.03f, 0.12f, 0.18f, 0.8f), false);
            Text help = Label("ControlsText", helpPanel.transform, font, "W / S  ход    ·    A / D  поворот", 26,
                SoftText, new Vector2(18f, -12f), new Vector2(484f, 44f));
            help.alignment = TextAnchor.MiddleCenter;

            Image warningPanel = Card("ExitWarningPanel", hud.transform, rounded, new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 125f), new Vector2(900f, 92f),
                new Color32(145, 67, 49, 244), false);
            warningPanel.enabled = false;
            Text warning = Label("ExitWarning", warningPanel.transform, font, "", 36, BrightText,
                new Vector2(22f, -12f), new Vector2(856f, 68f));
            warning.alignment = TextAnchor.MiddleCenter;
            warning.enabled = false;
            var warningUi = hud.AddComponent<ExitWarningPresenter>();
            Set(warningUi, "controller", exit);
            Set(warningUi, "warningText", warning);
            Set(warningUi, "warningBackground", warningPanel);
            hud.SetActive(true);
        }

        private static Image Card(string name, Transform parent, Sprite sprite, Vector2 anchor, Vector2 pivot,
            Vector2 position, Vector2 size, Color color, bool shadow = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), anchor, pivot, position, size);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
            if (shadow)
            {
                var effect = go.AddComponent<Shadow>();
                effect.effectColor = new Color(0f, 0f, 0f, 0.3f);
                effect.effectDistance = new Vector2(0f, -7f);
            }
            return image;
        }

        private static Image Stripe(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, Color color)
        {
            return Card(name, parent, sprite, new Vector2(0f, 1f), new Vector2(0f, 1f),
                position, size, color, false);
        }

        private static Text Label(string name, Transform parent, Font font, string value, int size,
            Color color, Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Place(go.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), position, dimensions);
            var label = go.GetComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.text = value;
            label.color = color;
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static Sprite RoundedSprite()
        {
            if (!File.Exists(RoundedSpritePath))
            {
                const int size = 64;
                const float radius = 16f;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var pixels = new Color[size * size];
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float qx = Mathf.Abs(x + 0.5f - size * 0.5f) - (size * 0.5f - radius);
                    float qy = Mathf.Abs(y + 0.5f - size * 0.5f) - (size * 0.5f - radius);
                    float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                    float distance = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(0.5f - distance));
                }
                texture.SetPixels(pixels);
                texture.Apply();
                File.WriteAllBytes(RoundedSpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(RoundedSpritePath);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(RoundedSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(18f, 18f, 18f, 18f);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(RoundedSpritePath);
        }

        private static void Set(Object target, string fieldName, Object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field == null) throw new System.MissingFieldException(target.GetType().Name, fieldName);
            field.SetValue(target, value);
            EditorUtility.SetDirty(target);
        }
    }
}
