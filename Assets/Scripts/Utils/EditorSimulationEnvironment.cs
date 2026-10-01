using UnityEngine;

namespace SurvivalShooter.Utils
{
    /// <summary>
    /// Provides a visual 3D simulation combat environment when running in the Unity Editor.
    /// Eliminates the black screen caused by absent mobile AR camera feeds during desktop playtesting.
    /// Automatically deactivates on mobile devices so AR video passthrough is unhindered.
    /// </summary>
    public class EditorSimulationEnvironment : MonoBehaviour
    {
        [Header("Environment Settings")]
        [SerializeField] private bool forceEnableInEditor = true;
        [SerializeField] private Color arenaFloorColor = new Color(0.08f, 0.12f, 0.18f);
        [SerializeField] private Color gridLineColor = new Color(0.1f, 0.5f, 0.8f, 0.35f);

        private GameObject arenaRoot;

        private void Awake()
        {
            // Only activate if in Editor or non-AR environment
            bool isMobileAR = !Application.isEditor && (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer);

            if (isMobileAR)
            {
                gameObject.SetActive(false);
                return;
            }

            BuildSimulationArena();
            ConfigureCameraForSimulation();
        }

        private void BuildSimulationArena()
        {
            arenaRoot = new GameObject("[Editor Simulation Arena]");
            arenaRoot.transform.SetParent(transform);
            arenaRoot.transform.position = new Vector3(0, -0.6f, 0);

            // Floor Plane
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "SimulationFloor";
            floor.transform.SetParent(arenaRoot.transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(2.5f, 1f, 2.5f); // 25x25 meter arena

            // Floor Material
            Shader shader = null;
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Mobile/Diffuse");
            if (shader == null) shader = Shader.Find("Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Color");

            Material floorMat = new Material(shader);
            if (floorMat.HasProperty("_BaseColor")) floorMat.SetColor("_BaseColor", arenaFloorColor);
            if (floorMat.HasProperty("_Color")) floorMat.SetColor("_Color", arenaFloorColor);
            if (floorMat.HasProperty("_Metallic")) floorMat.SetFloat("_Metallic", 0.4f);
            if (floorMat.HasProperty("_Smoothness")) floorMat.SetFloat("_Smoothness", 0.3f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Ensure floor has collider for raycasting
            var col = floor.GetComponent<MeshCollider>();
            if (col == null) floor.AddComponent<MeshCollider>();

            // Boundary decorative pillars
            Material pillarMat = new Material(shader);
            if (pillarMat.HasProperty("_BaseColor")) pillarMat.SetColor("_BaseColor", new Color(0.12f, 0.22f, 0.32f));
            if (pillarMat.HasProperty("_Color")) pillarMat.SetColor("_Color", new Color(0.12f, 0.22f, 0.32f));

            float radius = 7.0f;
            int count = 8;
            for (int i = 0; i < count; i++)
            {
                float angle = i * (Mathf.PI * 2f / count);
                GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pillar.name = $"Pillar_{i}";
                pillar.transform.SetParent(arenaRoot.transform);
                pillar.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, 1.5f, Mathf.Sin(angle) * radius);
                pillar.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
                pillar.GetComponent<Renderer>().sharedMaterial = pillarMat;
                Object.DestroyImmediate(pillar.GetComponent<Collider>());
            }
        }

        private void ConfigureCameraForSimulation()
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.05f, 0.08f, 0.12f); // Deep navy sci-fi ambient
            }
        }
    }
}
