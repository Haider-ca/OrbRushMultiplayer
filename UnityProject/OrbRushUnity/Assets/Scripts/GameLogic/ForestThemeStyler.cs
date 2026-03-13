using UnityEngine;

namespace OrbRush.GameLogic
{
    // Responsibility: Build a stylized forest arena and swap simple meshes for themed creatures.
    public class ForestThemeStyler : MonoBehaviour
    {
        private static bool _environmentBuilt;

        public static void EnsureEnvironment()
        {
            if (_environmentBuilt)
                return;

            _environmentBuilt = true;

            GameObject environmentRoot = new GameObject("ForestThemeEnvironment");
            CreateForestBorders(environmentRoot.transform);
            StyleGround();
            StyleCamera();
            CreateFireflies(environmentRoot.transform);
        }

        public static void StylePlayer(GameObject player, int playerId, bool isLocalPlayer)
        {
            if (!player || player.transform.Find("BugVisualRoot"))
                return;

            HideBaseRenderer(player);
            ConfigureCollider(player, 1.2f);

            PlayerPalette palette = GetPaletteForPlayer(playerId, isLocalPlayer);
            Color bodyColor = palette.bodyColor;
            Color shellColor = palette.shellColor;
            Color eyeColor = new Color(1f, 0.98f, 0.84f);

            GameObject root = new GameObject("BugVisualRoot");
            root.transform.SetParent(player.transform, false);
            root.transform.localPosition = new Vector3(0f, -0.1f, 0f);

            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Body", bodyColor,
                new Vector3(0f, 0.45f, 0f), new Vector3(0.95f, 0.55f, 1.25f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Thorax", shellColor,
                new Vector3(0f, 0.5f, 0.42f), new Vector3(0.7f, 0.45f, 0.65f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Head", shellColor,
                new Vector3(0f, 0.52f, 0.82f), new Vector3(0.52f, 0.38f, 0.42f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Shell", shellColor,
                new Vector3(0f, 0.62f, -0.1f), new Vector3(0.72f, 0.22f, 0.88f));

            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "EyeLeft", eyeColor,
                new Vector3(-0.14f, 0.64f, 0.98f), new Vector3(0.11f, 0.11f, 0.11f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "EyeRight", eyeColor,
                new Vector3(0.14f, 0.64f, 0.98f), new Vector3(0.11f, 0.11f, 0.11f));

            CreateLeg(root.transform, new Vector3(-0.38f, 0.28f, 0.35f), new Vector3(24f, -28f, -38f), shellColor);
            CreateLeg(root.transform, new Vector3(0.38f, 0.28f, 0.35f), new Vector3(24f, 28f, 38f), shellColor);
            CreateLeg(root.transform, new Vector3(-0.42f, 0.26f, -0.02f), new Vector3(8f, -18f, -54f), shellColor);
            CreateLeg(root.transform, new Vector3(0.42f, 0.26f, -0.02f), new Vector3(8f, 18f, 54f), shellColor);
            CreateLeg(root.transform, new Vector3(-0.34f, 0.24f, -0.38f), new Vector3(-18f, -26f, -65f), shellColor);
            CreateLeg(root.transform, new Vector3(0.34f, 0.24f, -0.38f), new Vector3(-18f, 26f, 65f), shellColor);

            CreateAntenna(root.transform, new Vector3(-0.12f, 0.7f, 0.97f), new Vector3(-24f, -18f, -8f), eyeColor);
            CreateAntenna(root.transform, new Vector3(0.12f, 0.7f, 0.97f), new Vector3(-24f, 18f, 8f), eyeColor);

            BugWobble wobble = player.GetComponent<BugWobble>();
            if (!wobble)
                wobble = player.AddComponent<BugWobble>();

            wobble.visualRoot = root.transform;
            wobble.wobbleOffset = Random.Range(0f, 10f);
        }

        public static void StyleOrb(GameObject orb)
        {
            if (!orb || orb.transform.Find("OrbMonsterRoot"))
                return;

            HideBaseRenderer(orb);
            ConfigureOrbTrigger(orb);

            GameObject root = new GameObject("OrbMonsterRoot");
            root.transform.SetParent(orb.transform, false);
            root.transform.localPosition = Vector3.zero;

            Color skinColor = new Color(0.72f, 0.38f, 0.78f);
            Color hornColor = new Color(0.2f, 0.16f, 0.24f);
            Color glowColor = new Color(1f, 0.7f, 0.28f);

            CreatePrimitivePart(root.transform, PrimitiveType.Capsule, "Body", skinColor,
                new Vector3(0f, 0.52f, 0f), new Vector3(0.72f, 0.5f, 0.72f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Head", skinColor,
                new Vector3(0f, 1.08f, 0f), new Vector3(0.68f, 0.52f, 0.68f));
            CreatePrimitivePart(root.transform, PrimitiveType.Sphere, "Eye", glowColor,
                new Vector3(0f, 1.12f, 0.28f), new Vector3(0.22f, 0.22f, 0.12f), true);
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "HornLeft", hornColor,
                new Vector3(-0.16f, 1.42f, -0.03f), new Vector3(0.1f, 0.22f, 0.1f), false, new Vector3(20f, 0f, 18f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "HornRight", hornColor,
                new Vector3(0.16f, 1.42f, -0.03f), new Vector3(0.1f, 0.22f, 0.1f), false, new Vector3(20f, 0f, -18f));

            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "ClawLeft", hornColor,
                new Vector3(-0.42f, 0.78f, 0.08f), new Vector3(0.08f, 0.2f, 0.08f), false, new Vector3(0f, 0f, 64f));
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "ClawRight", hornColor,
                new Vector3(0.42f, 0.78f, 0.08f), new Vector3(0.08f, 0.2f, 0.08f), false, new Vector3(0f, 0f, -64f));

            OrbFloat floatAnimation = orb.GetComponent<OrbFloat>();
            if (!floatAnimation)
                floatAnimation = orb.AddComponent<OrbFloat>();

            floatAnimation.visualRoot = root.transform;
            floatAnimation.floatOffset = Random.Range(0f, 10f);
        }

        private static void CreateForestBorders(Transform parent)
        {
            for (int i = 0; i < 12; i++)
            {
                float z = Mathf.Lerp(-12f, 12f, i / 11f);
                CreateTree(parent, new Vector3(-12.4f + Random.Range(-0.5f, 0.4f), 0f, z + Random.Range(-0.7f, 0.7f)),
                    1.1f + (i % 3) * 0.17f, new Color(0.16f, 0.46f, 0.3f));
                CreateTree(parent, new Vector3(12.4f + Random.Range(-0.4f, 0.5f), 0f, z + Random.Range(-0.7f, 0.7f)),
                    1.05f + ((i + 1) % 3) * 0.18f, new Color(0.2f, 0.52f, 0.34f));
            }

            for (int i = 0; i < 7; i++)
            {
                float x = Mathf.Lerp(-8f, 8f, i / 6f);
                CreateShrub(parent, new Vector3(x + Random.Range(-0.5f, 0.5f), 0f, -10.8f + Random.Range(-0.4f, 0.6f)),
                    new Color(0.34f, 0.6f, 0.36f));
                CreateShrub(parent, new Vector3(x + Random.Range(-0.5f, 0.5f), 0f, 10.8f + Random.Range(-0.6f, 0.4f)),
                    new Color(0.38f, 0.66f, 0.4f));
            }
        }

        private static void CreateTree(Transform parent, Vector3 position, float scale, Color leafColor)
        {
            GameObject tree = new GameObject("ForestTree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = position;

            CreatePrimitivePart(tree.transform, PrimitiveType.Cylinder, "Trunk", new Color(0.42f, 0.28f, 0.16f),
                new Vector3(0f, 1.15f * scale, 0f), new Vector3(0.38f, 1.2f * scale, 0.38f));
            CreatePrimitivePart(tree.transform, PrimitiveType.Cylinder, "LeavesLow", leafColor,
                new Vector3(0f, 2.1f * scale, 0f), new Vector3(1.7f * scale, 1.1f * scale, 1.7f * scale));
            CreatePrimitivePart(tree.transform, PrimitiveType.Cylinder, "LeavesHigh", leafColor * 1.16f,
                new Vector3(0f, 3.0f * scale, 0f), new Vector3(1.25f * scale, 1f * scale, 1.25f * scale));
        }

        private static void CreateShrub(Transform parent, Vector3 position, Color color)
        {
            GameObject shrub = new GameObject("ForestShrub");
            shrub.transform.SetParent(parent, false);
            shrub.transform.position = position;

            CreatePrimitivePart(shrub.transform, PrimitiveType.Sphere, "MoundA", color,
                new Vector3(-0.26f, 0.36f, 0f), new Vector3(0.72f, 0.56f, 0.72f));
            CreatePrimitivePart(shrub.transform, PrimitiveType.Sphere, "MoundB", color * 0.92f,
                new Vector3(0.22f, 0.34f, 0.06f), new Vector3(0.64f, 0.5f, 0.64f));
        }

        private static void CreateLeg(Transform parent, Vector3 localPosition, Vector3 localEulerAngles, Color color)
        {
            CreatePrimitivePart(parent, PrimitiveType.Cylinder, "Leg", color,
                localPosition, new Vector3(0.08f, 0.34f, 0.08f), false, localEulerAngles);
        }

        private static void CreateAntenna(Transform parent, Vector3 localPosition, Vector3 localEulerAngles, Color color)
        {
            CreatePrimitivePart(parent, PrimitiveType.Cylinder, "Antenna", color,
                localPosition, new Vector3(0.04f, 0.2f, 0.04f), true, localEulerAngles);
        }

        private static void CreateFireflies(Transform parent)
        {
            for (int i = 0; i < 12; i++)
            {
                GameObject fly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fly.name = "Firefly";
                fly.transform.SetParent(parent, false);
                fly.transform.position = new Vector3(Random.Range(-11f, 11f), Random.Range(1.8f, 3.8f), Random.Range(-11f, 11f));
                fly.transform.localScale = Vector3.one * Random.Range(0.08f, 0.14f);

                Object.Destroy(fly.GetComponent<Collider>());
                Renderer renderer = fly.GetComponent<Renderer>();
                renderer.sharedMaterial = CreateMaterial(new Color(1f, 0.94f, 0.55f), true);
                fly.AddComponent<FireflyDrift>().driftOffset = Random.Range(0f, 10f);
            }
        }

        private static void StyleGround()
        {
            GameObject ground = GameObject.Find("Plane");
            if (!ground)
                return;

            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer)
                renderer.sharedMaterial = CreateMaterial(new Color(0.48f, 0.66f, 0.58f));

            ground.transform.localScale = new Vector3(3.4f, 1f, 3.4f);
        }

        private static void StyleCamera()
        {
            if (!Camera.main)
                return;

            Camera.main.backgroundColor = new Color(0.46f, 0.72f, 0.78f);
        }

        private static void HideBaseRenderer(GameObject target)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer)
                renderer.enabled = false;
        }

        private static void ConfigureCollider(GameObject target, float height)
        {
            BoxCollider boxCollider = target.GetComponent<BoxCollider>();
            if (!boxCollider)
                return;

            boxCollider.center = new Vector3(0f, height * 0.5f, 0f);
            boxCollider.size = new Vector3(0.9f, height, 0.9f);
        }

        private static void ConfigureOrbTrigger(GameObject orb)
        {
            SphereCollider sphereCollider = orb.GetComponent<SphereCollider>();
            if (!sphereCollider)
                return;

            // The themed demon mesh is taller and wider than the original orb,
            // so expand the trigger to match what the player sees.
            sphereCollider.center = new Vector3(0f, 0.72f, 0f);
            sphereCollider.radius = 0.86f;
            sphereCollider.isTrigger = true;
        }

        private static GameObject CreatePrimitivePart(Transform parent, PrimitiveType primitiveType, string partName,
            Color color, Vector3 localPosition, Vector3 localScale, bool emissive = false, Vector3? localEulerAngles = null)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            if (localEulerAngles.HasValue)
                part.transform.localEulerAngles = localEulerAngles.Value;

            Object.Destroy(part.GetComponent<Collider>());

            Renderer renderer = part.GetComponent<Renderer>();
            renderer.sharedMaterial = CreateMaterial(color, emissive);
            return part;
        }

        private static Material CreateMaterial(Color color, bool emissive = false)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Glossiness", emissive ? 0.2f : 0.46f);

            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.2f);
            }

            return material;
        }

        private static PlayerPalette GetPaletteForPlayer(int playerId, bool isLocalPlayer)
        {
            PlayerPalette[] palettes =
            {
                new PlayerPalette(new Color(0.34f, 0.9f, 0.72f), new Color(0.14f, 0.52f, 0.38f)),
                new PlayerPalette(new Color(0.95f, 0.46f, 0.5f), new Color(0.58f, 0.16f, 0.2f)),
                new PlayerPalette(new Color(0.98f, 0.78f, 0.34f), new Color(0.7f, 0.44f, 0.08f)),
                new PlayerPalette(new Color(0.48f, 0.76f, 1f), new Color(0.18f, 0.4f, 0.72f)),
                new PlayerPalette(new Color(0.78f, 0.56f, 0.98f), new Color(0.42f, 0.24f, 0.62f)),
                new PlayerPalette(new Color(1f, 0.62f, 0.28f), new Color(0.72f, 0.32f, 0.06f)),
            };

            if (isLocalPlayer)
                return palettes[0];

            int paletteIndex = Mathf.Abs(playerId) % (palettes.Length - 1) + 1;
            return palettes[paletteIndex];
        }

        private readonly struct PlayerPalette
        {
            public readonly Color bodyColor;
            public readonly Color shellColor;

            public PlayerPalette(Color bodyColor, Color shellColor)
            {
                this.bodyColor = bodyColor;
                this.shellColor = shellColor;
            }
        }
    }

    public class BugWobble : MonoBehaviour
    {
        public Transform visualRoot;
        public float wobbleOffset;

        private void Update()
        {
            if (!visualRoot)
                return;

            float t = Time.time * 5.2f + wobbleOffset;
            visualRoot.localRotation = Quaternion.Euler(0f, Mathf.Sin(t * 0.5f) * 12f, Mathf.Sin(t) * 6f);
            visualRoot.localPosition = new Vector3(0f, -0.1f + Mathf.Abs(Mathf.Sin(t)) * 0.04f, 0f);
        }
    }

    public class OrbFloat : MonoBehaviour
    {
        public Transform visualRoot;
        public float floatOffset;

        private void Update()
        {
            if (!visualRoot)
                return;

            float t = Time.time * 2.5f + floatOffset;
            visualRoot.localPosition = new Vector3(0f, Mathf.Sin(t) * 0.14f, 0f);
            visualRoot.localRotation = Quaternion.Euler(0f, t * 55f, Mathf.Sin(t * 0.8f) * 6f);
        }
    }

    public class FireflyDrift : MonoBehaviour
    {
        public float driftOffset;
        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            float t = Time.time * 0.8f + driftOffset;
            transform.position = _startPosition + new Vector3(
                Mathf.Sin(t) * 0.4f,
                Mathf.Sin(t * 1.7f) * 0.18f,
                Mathf.Cos(t * 1.2f) * 0.4f
            );
        }
    }

}
