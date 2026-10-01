using UnityEngine;
using SurvivalShooter.Enemies;
using SurvivalShooter.Pooling;
using SurvivalShooter.AR;

namespace SurvivalShooter.Utils
{
    /// <summary>
    /// Procedurally constructs distinct 3D models, materials, and prefabs
    /// to satisfy asset requirements with visually distinguishable enemy silhouettes.
    /// </summary>
    public static class ProceduralModelBuilder
    {
        public static Material CreateMaterial(Color baseColor, Color emissionColor = default, float metallic = 0.5f, float smoothness = 0.6f)
        {
            // In Built-in Render Pipeline, standard shaders must be used to prevent magenta missing-shader artifacts
            Shader shader = null;
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null) shader = Shader.Find("Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            Material mat = new Material(shader != null ? shader : Shader.Find("Standard"));
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (emissionColor != default && emissionColor != Color.black)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emissionColor);
                }
            }

            return mat;
        }

        /// <summary>
        /// Builds the Melee Enemy: Aggressive cyber-beast with spiky blades and glowing red eyes.
        /// </summary>
        public static GameObject BuildMeleeEnemyPrefab()
        {
            GameObject root = new GameObject("MeleeEnemyPrefab");
            var col = root.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 0.45f, 0);
            col.radius = 0.35f;
            col.height = 0.9f;

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var meleeScript = root.AddComponent<MeleeEnemy>();

            // Materials
            Material bodyMat = CreateMaterial(new Color(0.18f, 0.18f, 0.2f), Color.black, 0.7f, 0.5f);
            Material redGlowMat = CreateMaterial(new Color(0.9f, 0.1f, 0.1f), new Color(1f, 0.1f, 0.1f) * 2f);
            Material bladeMat = CreateMaterial(new Color(0.85f, 0.85f, 0.9f), Color.black, 0.9f, 0.8f);

            GameObject modelContainer = new GameObject("VisualModel");
            modelContainer.transform.SetParent(root.transform);

            // Torso / Chassis
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.transform.SetParent(modelContainer.transform);
            torso.transform.localPosition = new Vector3(0, 0.45f, 0);
            torso.transform.localScale = new Vector3(0.5f, 0.4f, 0.65f);
            torso.GetComponent<Renderer>().sharedMaterial = bodyMat;
            Object.DestroyImmediate(torso.GetComponent<Collider>());

            // Head with predatory visor
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(modelContainer.transform);
            head.transform.localPosition = new Vector3(0, 0.65f, 0.35f);
            head.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
            head.GetComponent<Renderer>().sharedMaterial = bodyMat;
            Object.DestroyImmediate(head.GetComponent<Collider>());

            // Glowing Crimson Eye Visor
            GameObject eyeVisor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eyeVisor.transform.SetParent(head.transform);
            eyeVisor.transform.localPosition = new Vector3(0, 0.05f, 0.52f);
            eyeVisor.transform.localScale = new Vector3(0.85f, 0.35f, 0.15f);
            eyeVisor.GetComponent<Renderer>().sharedMaterial = redGlowMat;
            Object.DestroyImmediate(eyeVisor.GetComponent<Collider>());

            // Twin Melee Scythe Blades
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
                blade.transform.SetParent(modelContainer.transform);
                blade.transform.localPosition = new Vector3(side * 0.35f, 0.45f, 0.45f);
                blade.transform.localRotation = Quaternion.Euler(35f, side * 20f, side * -15f);
                blade.transform.localScale = new Vector3(0.06f, 0.12f, 0.65f);
                blade.GetComponent<Renderer>().sharedMaterial = bladeMat;
                Object.DestroyImmediate(blade.GetComponent<Collider>());
            }

            // 4 Quadruped cyber legs
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    leg.transform.SetParent(modelContainer.transform);
                    leg.transform.localPosition = new Vector3(x * 0.3f, 0.18f, z * 0.25f);
                    leg.transform.localRotation = Quaternion.Euler(x * 15f, 0f, z * -20f);
                    leg.transform.localScale = new Vector3(0.08f, 0.2f, 0.08f);
                    leg.GetComponent<Renderer>().sharedMaterial = bodyMat;
                    Object.DestroyImmediate(leg.GetComponent<Collider>());
                }
            }

            return root;
        }

        /// <summary>
        /// Builds the Shooter Enemy: Hovering tactical combat drone with tripod thrusters,
        /// dual plasma blaster cannons, and a pulsating cyan reactor core.
        /// </summary>
        public static GameObject BuildShooterEnemyPrefab()
        {
            GameObject root = new GameObject("ShooterEnemyPrefab");
            var col = root.AddComponent<SphereCollider>();
            col.center = new Vector3(0, 0.75f, 0);
            col.radius = 0.45f;

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var shooterScript = root.AddComponent<ShooterEnemy>();

            // Materials
            Material chassisMat = CreateMaterial(new Color(0.15f, 0.25f, 0.35f), Color.black, 0.8f, 0.7f);
            Material cyanCoreMat = CreateMaterial(new Color(0.1f, 0.85f, 1f), new Color(0.1f, 0.9f, 1f) * 2.5f);
            Material gunMat = CreateMaterial(new Color(0.1f, 0.1f, 0.12f), Color.black, 0.9f, 0.6f);

            GameObject modelContainer = new GameObject("VisualModel");
            modelContainer.transform.SetParent(root.transform);

            // Spherical chassis
            GameObject sphereBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereBody.transform.SetParent(modelContainer.transform);
            sphereBody.transform.localPosition = new Vector3(0, 0.75f, 0);
            sphereBody.transform.localScale = new Vector3(0.6f, 0.55f, 0.6f);
            sphereBody.GetComponent<Renderer>().sharedMaterial = chassisMat;
            Object.DestroyImmediate(sphereBody.GetComponent<Collider>());

            // Pulsating Cyan Plasma Reactor Core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.transform.SetParent(sphereBody.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            core.GetComponent<Renderer>().sharedMaterial = cyanCoreMat;
            Object.DestroyImmediate(core.GetComponent<Collider>());

            // Dual Blaster Cannons
            GameObject muzzleObj = new GameObject("MuzzlePoint");
            muzzleObj.transform.SetParent(root.transform);
            muzzleObj.transform.localPosition = new Vector3(0, 0.75f, 0.5f);

            for (int side = -1; side <= 1; side += 2)
            {
                GameObject gunBarrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                gunBarrel.transform.SetParent(modelContainer.transform);
                gunBarrel.transform.localPosition = new Vector3(side * 0.28f, 0.75f, 0.35f);
                gunBarrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                gunBarrel.transform.localScale = new Vector3(0.08f, 0.22f, 0.08f);
                gunBarrel.GetComponent<Renderer>().sharedMaterial = gunMat;
                Object.DestroyImmediate(gunBarrel.GetComponent<Collider>());
            }

            // Tripod Hover Repulsor Pods
            for (int i = 0; i < 3; i++)
            {
                float angle = i * (Mathf.PI * 2f / 3f);
                GameObject thruster = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                thruster.transform.SetParent(modelContainer.transform);
                thruster.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.35f, 0.42f, Mathf.Sin(angle) * 0.35f);
                thruster.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);
                thruster.GetComponent<Renderer>().sharedMaterial = chassisMat;
                Object.DestroyImmediate(thruster.GetComponent<Collider>());
            }

            return root;
        }

        /// <summary>
        /// Builds the Projectile Prefab for Object Pooling.
        /// </summary>
        public static GameObject BuildProjectilePrefab(bool isPlayer)
        {
            string name = isPlayer ? "PlayerProjectilePrefab" : "EnemyProjectilePrefab";
            GameObject proj = new GameObject(name);

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(proj.transform);
            sphere.transform.localScale = isPlayer ? new Vector3(0.12f, 0.12f, 0.25f) : new Vector3(0.2f, 0.2f, 0.2f);

            Color baseColor = isPlayer ? new Color(0.2f, 0.85f, 1f) : new Color(1f, 0.25f, 0.1f);
            Color emitColor = isPlayer ? new Color(0.3f, 0.95f, 1f) * 3f : new Color(1f, 0.3f, 0.1f) * 3f;

            sphere.GetComponent<Renderer>().sharedMaterial = CreateMaterial(baseColor, emitColor);

            var col = proj.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = isPlayer ? 0.15f : 0.2f;

            var rb = proj.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var projScript = proj.AddComponent<Projectile>();

            // Add TrailRenderer for sci-fi laser tracer
            var trail = proj.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = 0.08f;
            trail.endWidth = 0.0f;
            trail.material = CreateMaterial(baseColor, emitColor);

            return proj;
        }

        /// <summary>
        /// Builds the Custom Plane Tracker reticle featuring student full name.
        /// </summary>
        public static GameObject BuildCustomPlaneTrackerPrefab(string studentName = "Inema Amanda Leslie")
        {
            GameObject tracker = new GameObject("CustomPlaneTrackerPrefab");

            // Outer glowing reticle ring
            GameObject outerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            outerRing.name = "OuterRing";
            outerRing.transform.SetParent(tracker.transform);
            outerRing.transform.localPosition = Vector3.zero;
            outerRing.transform.localScale = new Vector3(1.3f, 0.008f, 1.3f);
            Object.DestroyImmediate(outerRing.GetComponent<Collider>());
            Material ringMat = CreateMaterial(new Color(0f, 0.8f, 1f, 0.85f), new Color(0f, 0.9f, 1f) * 1.5f);
            outerRing.GetComponent<Renderer>().sharedMaterial = ringMat;

            // Inner dark tech baseplate for high contrast against any floor surface
            GameObject basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlate.name = "BasePlate";
            basePlate.transform.SetParent(tracker.transform);
            basePlate.transform.localPosition = new Vector3(0, 0.005f, 0);
            basePlate.transform.localScale = new Vector3(1.15f, 0.01f, 1.15f);
            Object.DestroyImmediate(basePlate.GetComponent<Collider>());
            Material plateMat = CreateMaterial(new Color(0.04f, 0.08f, 0.14f, 0.95f), Color.black, 0.7f, 0.4f);
            basePlate.GetComponent<Renderer>().sharedMaterial = plateMat;

            // Elevated solid name badge plaque
            GameObject badgePlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            badgePlate.name = "NameBadgePlate";
            badgePlate.transform.SetParent(tracker.transform);
            badgePlate.transform.localPosition = new Vector3(0, 0.025f, 0);
            badgePlate.transform.localScale = new Vector3(0.95f, 0.02f, 0.35f);
            Object.DestroyImmediate(badgePlate.GetComponent<Collider>());
            Material badgeMat = CreateMaterial(new Color(0.02f, 0.05f, 0.1f, 1.0f), new Color(0f, 0.7f, 1f) * 0.8f, 0.8f, 0.5f);
            badgePlate.GetComponent<Renderer>().sharedMaterial = badgeMat;

            // Visualizer script
            var viz = tracker.AddComponent<CustomPlaneVisualizer>();

            // 3D Text displaying student's full name prominently
            GameObject textGo = new GameObject("StudentNameBadge");
            textGo.transform.SetParent(tracker.transform);
            textGo.transform.localPosition = new Vector3(0, 0.045f, 0);
            textGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var textMesh = textGo.AddComponent<TextMesh>();
            textMesh.characterSize = 0.045f;
            textMesh.fontSize = 54;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontStyle = FontStyle.Bold;
            textMesh.color = new Color(0.2f, 1.0f, 1.0f, 1f);
            textMesh.text = $"{studentName.ToUpper()}\n<size=28>AR PLANE TRACKER</size>";

            return tracker;
        }
    }
}
