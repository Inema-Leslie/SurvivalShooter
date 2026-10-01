#if UNITY_EDITOR
using UnityEngine;
using SurvivalShooter.Core;

namespace SurvivalShooter.Utils
{
    /// <summary>
    /// Editor-only utility enabling Mouse Look and WASD navigation when playtesting in Unity Editor.
    /// Provides PC controls so the instructor or developer can look around and move in AR space.
    /// Excluded from mobile production builds.
    /// </summary>
    public class EditorCameraFreelook : MonoBehaviour
    {
        [Header("Editor PC Controls")]
        [SerializeField] private float lookSensitivity = 2.5f;
        [SerializeField] private float moveSpeed = 3.0f;

        private float rotationX = 0f;
        private float rotationY = 0f;
        private bool isRightClickHeld = false;

        private void Start()
        {
            Vector3 angles = transform.eulerAngles;
            rotationX = angles.y;
            rotationY = -angles.x;
        }

        private void Update()
        {
            // Right-click drag or active combat freelook
            if (Input.GetMouseButtonDown(1))
            {
                isRightClickHeld = true;
            }
            if (Input.GetMouseButtonUp(1))
            {
                isRightClickHeld = false;
            }

            // Mouse Look
            if (isRightClickHeld || (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing && !Input.GetMouseButton(0)))
            {
                float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

                rotationX += mouseX;
                rotationY -= mouseY;
                rotationY = Mathf.Clamp(rotationY, -85f, 85f);

                transform.rotation = Quaternion.Euler(-rotationY, rotationX, 0f);
            }

            // WASD Movement
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                Vector3 moveDir = (transform.forward * v + transform.right * h);
                moveDir.y = 0; // Stay upright
                transform.position += moveDir.normalized * (moveSpeed * Time.deltaTime);
            }
        }
    }
}
#endif
