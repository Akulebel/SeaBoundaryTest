using System.Collections.Generic;
using System.IO;
using SeaBoundary;
using UnityEditor;
using UnityEngine;

namespace SeaBoundary.Editor
{
    internal static class MainSceneVisualsBuilder
    {
        private const string Root = "Assets/_Project";
        private const string SeaTexturePath = Root + "/Textures/SeaWaves.png";
        private static readonly Vector2[] HullOutline =
        {
            new Vector2(-0.62f, -1.90f), new Vector2(-0.95f, -1.45f),
            new Vector2(-1.03f, 0.35f), new Vector2(-0.67f, 1.52f),
            new Vector2(0f, 2.28f), new Vector2(0.67f, 1.52f),
            new Vector2(1.03f, 0.35f), new Vector2(0.95f, -1.45f),
            new Vector2(0.62f, -1.90f)
        };

        public static void BuildWater(Material material)
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
            water.name = "Water";
            water.transform.localScale = new Vector3(8f, 1f, 8f);
            var renderer = water.GetComponent<MeshRenderer>();
            material.SetTexture("_BaseMap", SeaTexture());
            material.SetTextureScale("_BaseMap", new Vector2(4f, 4f));
            EditorUtility.SetDirty(material);
            renderer.sharedMaterial = material;
            var surface = water.AddComponent<SeaSurface>();
            Set(surface, "surface", renderer);
        }

        public static void BuildShip(Transform ship, Material hull, Material deck, Material trim,
            Material sail, Material mast, Material cabin, Material glass)
        {
            var model = new GameObject("Model");
            model.transform.SetParent(ship, false);
            model.transform.localScale = Vector3.one * 1.23f;
            Transform visual = model.transform;
            MeshPart("HullShape", visual, HullMesh(), hull);
            MeshPart("Deck", visual, DeckMesh(), deck);

            var rim = new GameObject("Gunwale");
            rim.transform.SetParent(visual, false);
            var line = rim.AddComponent<LineRenderer>();
            line.sharedMaterial = trim;
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = HullOutline.Length;
            line.startWidth = line.endWidth = 0.075f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            for (int i = 0; i < HullOutline.Length; i++)
                line.SetPosition(i, new Vector3(HullOutline[i].x, 0.28f, HullOutline[i].y));

            Part(PrimitiveType.Cube, "Cabin", visual, new Vector3(0f, 0.42f, -1.12f),
                new Vector3(1.14f, 0.34f, 0.72f), cabin);
            Part(PrimitiveType.Cube, "CabinWindow", visual, new Vector3(0f, 0.60f, -1.12f),
                new Vector3(0.58f, 0.025f, 0.28f), glass);
            Part(PrimitiveType.Cylinder, "Mast", visual, new Vector3(0f, 1.24f, -0.12f),
                new Vector3(0.09f, 1.0f, 0.09f), mast);
            var yard = Part(PrimitiveType.Cylinder, "Yard", visual, new Vector3(0f, 1.95f, -0.12f),
                new Vector3(0.055f, 1.1f, 0.055f), mast);
            yard.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            MeshPart("FixedSail", visual, SailMesh(), sail);
            var bowsprit = Part(PrimitiveType.Cylinder, "Bowsprit", visual, new Vector3(0f, 0.37f, 2.23f),
                new Vector3(0.04f, 0.55f, 0.04f), mast);
            bowsprit.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void MeshPart(string name, Transform parent, Mesh mesh, Material material)
        {
            var part = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            part.transform.SetParent(parent, false);
            part.GetComponent<MeshFilter>().sharedMesh = mesh;
            part.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 position,
            Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        private static Mesh HullMesh()
        {
            var vertices = new List<Vector3> { new Vector3(0f, 0.17f, 0f) };
            var triangles = new List<int>();
            int count = HullOutline.Length;
            for (int i = 0; i < count; i++)
            {
                Vector2 point = HullOutline[i];
                vertices.Add(new Vector3(point.x, 0.17f, point.y));
            }
            for (int i = 0; i < count; i++)
            {
                Vector2 point = HullOutline[i];
                vertices.Add(new Vector3(point.x * 0.78f, -0.28f, point.y * 0.94f));
            }
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                triangles.Add(0); triangles.Add(1 + i); triangles.Add(1 + next);
                int top = 1 + i, topNext = 1 + next;
                int bottom = 1 + count + i, bottomNext = 1 + count + next;
                triangles.Add(top); triangles.Add(bottom); triangles.Add(topNext);
                triangles.Add(topNext); triangles.Add(bottom); triangles.Add(bottomNext);
            }
            return SaveMesh("ShipHull", vertices.ToArray(), triangles.ToArray());
        }

        private static Mesh DeckMesh()
        {
            int count = HullOutline.Length;
            var vertices = new Vector3[count + 1];
            var triangles = new int[count * 3];
            vertices[0] = new Vector3(0f, 0.22f, 0f);
            for (int i = 0; i < count; i++)
            {
                vertices[i + 1] = new Vector3(HullOutline[i].x * 0.88f, 0.22f, HullOutline[i].y * 0.91f);
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 1) % count + 1;
            }
            return SaveMesh("ShipDeck", vertices, triangles);
        }

        private static Mesh SailMesh()
        {
            // +Z points to the bow. The lower cloth billows forward of the fixed yard.
            var face = new[]
            {
                new Vector3(-1.03f, 1.92f, -0.12f), new Vector3(0f, 2.03f, -0.30f),
                new Vector3(1.03f, 1.92f, -0.12f), new Vector3(-0.85f, 0.78f, 0.40f),
                new Vector3(0f, 0.72f, 1.35f), new Vector3(0.85f, 0.78f, 0.40f)
            };
            var front = new[] { 0, 3, 1, 1, 3, 4, 1, 4, 2, 2, 4, 5 };
            var vertices = new Vector3[face.Length * 2];
            face.CopyTo(vertices, 0);
            face.CopyTo(vertices, face.Length);
            var triangles = new int[front.Length * 2];
            for (int i = 0; i < front.Length; i += 3)
            {
                triangles[i] = front[i];
                triangles[i + 1] = front[i + 1];
                triangles[i + 2] = front[i + 2];
                triangles[front.Length + i] = face.Length + front[i + 2];
                triangles[front.Length + i + 1] = face.Length + front[i + 1];
                triangles[front.Length + i + 2] = face.Length + front[i];
            }
            return SaveMesh("ShipSail", vertices, triangles);
        }

        private static Mesh SaveMesh(string name, Vector3[] vertices, int[] triangles)
        {
            string path = Root + "/Meshes/" + name + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = name };
                AssetDatabase.CreateAsset(mesh, path);
            }
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static Texture2D SeaTexture()
        {
            const int size = 256;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size;
                float v = y / (float)size;
                float swell = Mathf.Sin(2f * Mathf.PI * (2f * u + v)) * 0.005f;
                pixels[y * size + x] = new Color(0.055f + swell, 0.20f + swell, 0.27f + swell, 1f);
            }
            var random = new System.Random(6045);
            var glint = new Color(0.20f, 0.37f, 0.44f, 1f);
            for (int i = 0; i < 85; i++)
            {
                int centerX = random.Next(size);
                int centerY = random.Next(size);
                int halfLength = random.Next(7, 23);
                float slope = (float)(random.NextDouble() * 0.30 - 0.15);
                float arch = (float)(random.NextDouble() * 4.0 - 2.0);
                for (int step = -halfLength; step <= halfLength; step++)
                {
                    float t = step / (float)halfLength;
                    float fade = Mathf.Pow(1f - Mathf.Abs(t), 1.3f);
                    int px = Wrap(centerX + step, size);
                    int py = Mathf.RoundToInt(centerY + slope * step + arch * (1f - t * t));
                    for (int thickness = -1; thickness <= 1; thickness++)
                    {
                        int index = Wrap(py + thickness, size) * size + px;
                        float opacity = fade * (thickness == 0 ? 0.48f : 0.12f);
                        pixels[index] = Color.Lerp(pixels[index], glint, opacity);
                    }
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(SeaTexturePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(SeaTexturePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SeaTexturePath);
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(SeaTexturePath);
        }

        private static int Wrap(int value, int size) => (value % size + size) % size;

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
