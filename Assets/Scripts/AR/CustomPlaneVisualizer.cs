using UnityEngine;

namespace SurvivalShooter.AR
{
    /// <summary>
    /// Custom Plane Tracker visualizer replacing default AR plane visuals.
    /// Strictly displays the student's full name ("Inema Amanda Leslie")
    /// prominently on the detected plane surface.
    /// Displays only when a plane is actively detected and tracked.
    /// </summary>
    public class CustomPlaneVisualizer : MonoBehaviour
    {
        [Header("Student Identification (Mandatory)")]
        [SerializeField] private string studentName = "Inema Amanda Leslie";
        [SerializeField] private TextMesh nameTextMesh;

        [Header("Visual Styling")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private LineRenderer borderRenderer;
        [SerializeField] private Color trackerColor = new Color(0.1f, 0.8f, 1f, 0.45f);
        [SerializeField] private Color borderColor = new Color(0.2f, 0.95f, 1f, 0.9f);

        private float pulseTimer;

        private void Awake()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

            // Setup or locate 3D TextMesh for student name
            if (nameTextMesh == null)
            {
                nameTextMesh = GetComponentInChildren<TextMesh>();
                if (nameTextMesh == null)
                {
                    GameObject textGo = new GameObject("PlaneStudentNameBadge");
                    textGo.transform.SetParent(transform);
                    textGo.transform.localPosition = new Vector3(0, 0.02f, 0);
                    textGo.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    nameTextMesh = textGo.AddComponent<TextMesh>();
                    nameTextMesh.characterSize = 0.06f;
                    nameTextMesh.fontSize = 52;
                    nameTextMesh.alignment = TextAlignment.Center;
                    nameTextMesh.anchor = TextAnchor.MiddleCenter;
                    nameTextMesh.fontStyle = FontStyle.Bold;
                    nameTextMesh.color = Color.white;
                }
            }

            nameTextMesh.text = $"{studentName.ToUpper()}\n<size=28>AR PLANE TRACKED</size>";
        }

        private void Update()
        {
            // Vibrant glowing pulsation effect for futuristic cyber grid aesthetic
            pulseTimer += Time.deltaTime * 3.0f;
            float pulse = 0.85f + Mathf.PingPong(pulseTimer, 0.15f);

            if (meshRenderer != null && meshRenderer.material.HasProperty("_Color"))
            {
                Color c = trackerColor;
                c.a = pulse;
                meshRenderer.material.color = c;
            }

            // Always align text and badge to face the AR camera comfortably
            if (nameTextMesh != null && Camera.main != null)
            {
                Vector3 camPos = Camera.main.transform.position;
                Vector3 dir = (camPos - nameTextMesh.transform.position);
                dir.y = 0; // Keep horizontal flat rotation
                if (dir.sqrMagnitude > 0.01f)
                {
                    nameTextMesh.transform.rotation = Quaternion.LookRotation(Vector3.up, -dir);
                }
            }
        }
    }
}
