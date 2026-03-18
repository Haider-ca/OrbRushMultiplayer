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
            CreateSkyAndSun(environmentRoot.transform);
            CreateStars(environmentRoot.transform);
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
            CreatePrimitivePart(root.transform, PrimitiveType.Cylinder, "BellyGlow", new Color(0.92f, 0.42f, 0.78f),
                new Vector3(0f, 0.64f, 0.16f), new Vector3(0.2f, 0.08f, 0.28f), true, new Vector3(90f, 0f, 0f));
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

            for (int i = 0; i < 10; i++)
            {
                float z = Mathf.Lerp(-10.8f, 10.8f, i / 9f);
                CreateTree(parent, new Vector3(-13.75f + Random.Range(-0.35f, 0.3f), 0f, z + Random.Range(-0.55f, 0.55f)),
                    0.92f + (i % 3) * 0.08f, new Color(0.15f, 0.4f, 0.28f));
                CreateTree(parent, new Vector3(13.75f + Random.Range(-0.3f, 0.35f), 0f, z + Random.Range(-0.55f, 0.55f)),
                    0.9f + ((i + 1) % 3) * 0.08f, new Color(0.17f, 0.42f, 0.29f));
            }

            for (int i = 0; i < 7; i++)
            {
                float z = Mathf.Lerp(-12.4f, 13.8f, i / 6f);
                CreateTree(parent, new Vector3(-15.1f + Random.Range(-0.45f, 0.35f), 0f, z + Random.Range(-0.6f, 0.6f)),
                    1.18f + (i % 2) * 0.12f, new Color(0.14f, 0.42f, 0.28f));
                CreateTree(parent, new Vector3(15.1f + Random.Range(-0.35f, 0.45f), 0f, z + Random.Range(-0.6f, 0.6f)),
                    1.14f + ((i + 1) % 2) * 0.14f, new Color(0.16f, 0.44f, 0.3f));
            }

            for (int i = 0; i < 10; i++)
            {
                float x = Mathf.Lerp(-10.5f, 10.5f, i / 9f);
                CreateTree(parent, new Vector3(x + Random.Range(-0.45f, 0.45f), 0f, 13.4f + Random.Range(-0.35f, 0.45f)),
                    0.98f + (i % 3) * 0.12f, new Color(0.18f, 0.48f, 0.31f));
            }

            for (int i = 0; i < 12; i++)
            {
                float x = Mathf.Lerp(-11.8f, 11.8f, i / 11f);
                CreateTree(parent, new Vector3(x + Random.Range(-0.28f, 0.28f), 0f, 11.75f + Random.Range(-0.22f, 0.22f)),
                    0.7f + (i % 2) * 0.08f, new Color(0.2f, 0.5f, 0.33f));
            }

            for (int i = 0; i < 8; i++)
            {
                float x = Mathf.Lerp(-9f, 9f, i / 7f);
                CreateTree(parent, new Vector3(x + Random.Range(-0.35f, 0.35f), 0f, 15.1f + Random.Range(-0.25f, 0.35f)),
                    0.82f + (i % 2) * 0.1f, new Color(0.16f, 0.42f, 0.28f));
            }

            CreateTree(parent, new Vector3(-12.9f, 0f, 14.7f), 1.04f, new Color(0.15f, 0.43f, 0.29f));
            CreateTree(parent, new Vector3(-14.2f, 0f, 15.6f), 1.18f, new Color(0.14f, 0.39f, 0.27f));
            CreateTree(parent, new Vector3(12.8f, 0f, 14.8f), 1.02f, new Color(0.18f, 0.45f, 0.3f));
            CreateTree(parent, new Vector3(14.3f, 0f, 15.5f), 1.16f, new Color(0.16f, 0.41f, 0.28f));

            for (int i = 0; i < 7; i++)
            {
                float x = Mathf.Lerp(-8f, 8f, i / 6f);
                CreateShrub(parent, new Vector3(x + Random.Range(-0.5f, 0.5f), 0f, -10.8f + Random.Range(-0.4f, 0.6f)),
                    new Color(0.34f, 0.6f, 0.36f));
                CreateShrub(parent, new Vector3(x + Random.Range(-0.5f, 0.5f), 0f, 10.8f + Random.Range(-0.6f, 0.4f)),
                    new Color(0.38f, 0.66f, 0.4f));
            }

            for (int i = 0; i < 9; i++)
            {
                float x = Mathf.Lerp(-10f, 10f, i / 8f);
                CreateShrub(parent, new Vector3(x + Random.Range(-0.35f, 0.35f), 0f, 12.2f + Random.Range(-0.25f, 0.35f)),
                    new Color(0.28f, 0.54f, 0.32f));
            }

            for (int i = 0; i < 10; i++)
            {
                float z = Mathf.Lerp(-11f, 11f, i / 9f);
                CreateShrub(parent, new Vector3(-12.2f + Random.Range(-0.25f, 0.25f), 0f, z + Random.Range(-0.35f, 0.35f)),
                    new Color(0.24f, 0.48f, 0.29f));
                CreateShrub(parent, new Vector3(12.2f + Random.Range(-0.25f, 0.25f), 0f, z + Random.Range(-0.35f, 0.35f)),
                    new Color(0.26f, 0.5f, 0.31f));
            }

            CreateShrub(parent, new Vector3(-4.8f, 0f, 3.6f), new Color(0.28f, 0.54f, 0.32f));
            CreateShrub(parent, new Vector3(4.6f, 0f, 2.8f), new Color(0.3f, 0.56f, 0.34f));
            CreateShrub(parent, new Vector3(-5.2f, 0f, -3.4f), new Color(0.26f, 0.52f, 0.31f));
            CreateShrub(parent, new Vector3(4.9f, 0f, -4.2f), new Color(0.29f, 0.55f, 0.33f));
            CreateShrub(parent, new Vector3(-1.8f, 0f, 5.2f), new Color(0.32f, 0.58f, 0.35f));
            CreateShrub(parent, new Vector3(2.2f, 0f, -5.6f), new Color(0.28f, 0.53f, 0.32f));
        }

        private static void CreateTree(Transform parent, Vector3 position, float scale, Color leafColor)
        {
            GameObject tree = new GameObject("ForestTree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = position;
            tree.transform.localEulerAngles = new Vector3(0f, Random.Range(-16f, 16f), 0f);

            float scaleJitter = Random.Range(0.9f, 1.12f);
            scale *= scaleJitter;
            Color lowLeafColor = leafColor * Random.Range(0.92f, 1.06f);
            Color highLeafColor = lowLeafColor * Random.Range(1.04f, 1.14f);
            Color trunkColor = new Color(0.42f, 0.28f, 0.16f) * Random.Range(0.9f, 1.06f);

            CreatePrimitivePart(tree.transform, PrimitiveType.Cylinder, "Trunk", trunkColor,
                new Vector3(0f, 1.15f * scale, 0f), new Vector3(0.38f, 1.2f * scale, 0.38f));
            CreatePrimitivePart(tree.transform, PrimitiveType.Cylinder, "LeavesLow", lowLeafColor,
                new Vector3(0f, 2.1f * scale, 0f), new Vector3(1.7f * scale, 1.1f * scale, 1.7f * scale));
            CreatePrimitivePart(tree.transform, PrimitiveType.Cylinder, "LeavesHigh", highLeafColor,
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
            for (int i = 0; i < 26; i++)
            {
                GameObject fly = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fly.name = "Firefly";
                fly.transform.SetParent(parent, false);
                fly.transform.position = new Vector3(Random.Range(-11f, 11f), Random.Range(1.8f, 3.8f), Random.Range(-11f, 11f));
                fly.transform.localScale = Vector3.one * Random.Range(0.04f, 0.09f);

                Object.Destroy(fly.GetComponent<Collider>());
                Renderer renderer = fly.GetComponent<Renderer>();
                renderer.sharedMaterial = CreateMaterial(new Color(1f, 0.88f, 0.24f), true);

                FireflyDrift drift = fly.AddComponent<FireflyDrift>();
                drift.driftOffset = Random.Range(0f, 10f);

                FireflyBlink blink = fly.AddComponent<FireflyBlink>();
                blink.blinkOffset = Random.Range(0f, 10f);
            }
        }

        private static void CreateSkyAndSun(Transform parent)
        {
            GameObject moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            moon.name = "Moon";
            moon.transform.SetParent(parent, false);
            moon.transform.position = new Vector3(-10f, 22f, 44f);
            moon.transform.localScale = Vector3.one * 4.8f;
            Object.Destroy(moon.GetComponent<Collider>());

            Renderer moonRenderer = moon.GetComponent<Renderer>();
            moonRenderer.sharedMaterial = CreateMaterial(new Color(0.78f, 0.86f, 1f), true);

            GameObject moonlightObject = new GameObject("MoonLight");
            moonlightObject.transform.SetParent(parent, false);
            moonlightObject.transform.rotation = Quaternion.Euler(26f, -18f, 0f);

            Light moonlight = moonlightObject.AddComponent<Light>();
            moonlight.type = LightType.Directional;
            moonlight.color = new Color(0.72f, 0.8f, 1f);
            moonlight.intensity = 0.78f;
            moonlight.shadows = LightShadows.Soft;
            moonlight.shadowStrength = 0.36f;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.22f, 0.34f);
            RenderSettings.ambientEquatorColor = new Color(0.12f, 0.18f, 0.2f);
            RenderSettings.ambientGroundColor = new Color(0.06f, 0.09f, 0.08f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.14f, 0.19f, 0.24f);
            RenderSettings.fogStartDistance = 28f;
            RenderSettings.fogEndDistance = 78f;
        }

        private static void CreateStars(Transform parent)
        {
            for (int i = 0; i < 42; i++)
            {
                GameObject star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                star.name = "Star";
                star.transform.SetParent(parent, false);
                star.transform.position = new Vector3(
                    Random.Range(-44f, 44f),
                    Random.Range(22f, 34f),
                    Random.Range(28f, 62f));
                star.transform.localScale = Vector3.one * Random.Range(0.12f, 0.28f);

                Object.Destroy(star.GetComponent<Collider>());
                Renderer renderer = star.GetComponent<Renderer>();
                renderer.sharedMaterial = CreateMaterial(new Color(0.92f, 0.96f, 1f), true);
                StarTwinkle twinkle = star.AddComponent<StarTwinkle>();
                twinkle.twinkleOffset = Random.Range(0f, 10f);
            }
        }

        private static void StyleGround()
        {
            GameObject ground = GameObject.Find("Plane");
            if (!ground)
                return;

            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer)
                renderer.sharedMaterial = CreateMaterial(new Color(0.32f, 0.42f, 0.4f));

            ground.transform.localScale = new Vector3(3.4f, 1f, 3.4f);
        }

        private static void StyleCamera()
        {
            if (!Camera.main)
                return;

            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.12f, 0.16f, 0.26f);
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

    public class FireflyBlink : MonoBehaviour
    {
        public float blinkOffset;

        private Vector3 _baseScale;
        private Material _materialInstance;

        private void Start()
        {
            _baseScale = transform.localScale;

            Renderer renderer = GetComponent<Renderer>();
            if (renderer)
                _materialInstance = renderer.material;
        }

        private void Update()
        {
            float t = Time.time * 3.2f + blinkOffset;
            float blink = 0.35f + Mathf.Abs(Mathf.Sin(t)) * 1.15f;

            transform.localScale = _baseScale * (0.75f + blink * 0.22f);

            if (_materialInstance)
                _materialInstance.SetColor("_EmissionColor", new Color(1f, 0.82f, 0.18f) * blink * 2.4f);
        }
    }

    public class StarTwinkle : MonoBehaviour
    {
        public float twinkleOffset;
        private Vector3 _baseScale;

        private void Start()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            float t = Time.time * 1.4f + twinkleOffset;
            float pulse = 0.85f + Mathf.Abs(Mathf.Sin(t)) * 0.4f;
            transform.localScale = _baseScale * pulse;
        }
    }

}
