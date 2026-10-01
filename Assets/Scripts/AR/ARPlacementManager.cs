using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using SurvivalShooter.Core;

namespace SurvivalShooter.AR
{
    /// <summary>
    /// Manages AR horizontal plane detection, raycasting, placement reticle,
    /// and tap-to-place game anchoring.
    /// Includes Editor testing fallback for PC development without a mobile AR device.
    /// </summary>
    public class ARPlacementManager : MonoBehaviour
    {
        [Header("Placement Reticle & Anchor")]
        [SerializeField] private GameObject placementReticlePrefab;
        [SerializeField] private GameObject gameWorldAnchorPrefab;
        [SerializeField] private Transform spawnedAnchor;

        [Header("Plane Tracker Name Badge")]
        [SerializeField] private string studentName = "Inema Amanda Leslie";

        [Header("AR Foundation Managers")]
        [SerializeField] private ARRaycastManager arRaycastManager;
        [SerializeField] private ARPlaneManager arPlaneManager;

        private GameObject activeReticle;
        private bool hasPlacedGameWorld = false;
        private Pose currentPlacementPose;
        private bool isPlacementValid = false;
        private static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

        private void Awake()
        {
            FindARComponents();
        }

        private void FindARComponents()
        {
            if (arRaycastManager == null) arRaycastManager = FindAnyObjectByType<ARRaycastManager>();
            if (arPlaneManager == null) arPlaneManager = FindAnyObjectByType<ARPlaneManager>();
        }

        private void Start()
        {
            if (placementReticlePrefab != null)
            {
                activeReticle = Instantiate(placementReticlePrefab);
                activeReticle.SetActive(false);

                // Ensure student's full name is visibly displayed on the reticle / tracker
                ApplyNameToTracker(activeReticle);
            }
        }

        private void OnEnable()
        {
            GameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (state == GameState.PlaneScanning || state == GameState.PlacementReady)
            {
                if (!hasPlacedGameWorld)
                {
                    SetPlaneDetectionEnabled(true);
                }
            }
            else
            {
                if (activeReticle != null)
                {
                    activeReticle.SetActive(false);
                }
            }
        }

        private void Update()
        {
            if (GameManager.Instance == null) return;

            GameState state = GameManager.Instance.CurrentState;
            if (state != GameState.PlaneScanning && state != GameState.PlacementReady)
            {
                return;
            }

            if (hasPlacedGameWorld) return;

            UpdatePlacementPose();
            UpdatePlacementIndicator();
            HandlePlacementInput();
        }

        private void UpdatePlacementPose()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Try AR Foundation Raycast via reflection for seamless multi-version compatibility
            bool arHitSuccess = false;

            if (arRaycastManager != null)
            {
                var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                if (arRaycastManager.Raycast(screenCenter, s_Hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
                {
                    if (s_Hits.Count > 0)
                    {
                        currentPlacementPose = s_Hits[0].pose;
                        arHitSuccess = true;
                    }
                }
            }

            // Fallback for Editor simulation or standard physics planes
            if (!arHitSuccess)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(ray, out RaycastHit hitInfo, 10f))
                {
                    currentPlacementPose.position = hitInfo.point;
                    Vector3 forward = cam.transform.forward;
                    forward.y = 0;
                    if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
                    currentPlacementPose.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
                    isPlacementValid = true;
                    return;
                }
                else
                {
                    // Simulated floor at Y = -0.6m in front of camera for instant testing
                    Plane floorPlane = new Plane(Vector3.up, new Vector3(0, -0.6f, 0));
                    if (floorPlane.Raycast(ray, out float enter))
                    {
                        currentPlacementPose.position = ray.GetPoint(enter);
                        Vector3 forward = cam.transform.forward;
                        forward.y = 0;
                        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
                        currentPlacementPose.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
                        isPlacementValid = true;
                        return;
                    }
                }
            }

            isPlacementValid = arHitSuccess;
        }

        private void UpdatePlacementIndicator()
        {
            if (activeReticle == null) return;

            if (isPlacementValid)
            {
                activeReticle.SetActive(true);
                activeReticle.transform.SetPositionAndRotation(currentPlacementPose.position, currentPlacementPose.rotation);
                GameManager.Instance.SetPlacementReady();
            }
            else
            {
                activeReticle.SetActive(false);
            }
        }

        private void HandlePlacementInput()
        {
            if (!isPlacementValid || hasPlacedGameWorld) return;

            bool tapDetected = false;

            // Touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began && !IsPointerOverUI(touch.fingerId))
                {
                    tapDetected = true;
                }
            }
            // Mouse input for editor
            else if (Input.GetMouseButtonDown(0) && !IsPointerOverUI(-1))
            {
                tapDetected = true;
            }

            if (tapDetected)
            {
                PlaceGameWorld();
            }
        }

        /// <summary>
        /// Places the game world anchor on the detected horizontal plane.
        /// Enforces single placement (subsequent taps do nothing).
        /// Stops plane detection as encouraged.
        /// </summary>
        private void PlaceGameWorld()
        {
            if (hasPlacedGameWorld) return;
            hasPlacedGameWorld = true;

            // Instantiate or position the anchored game world
            if (gameWorldAnchorPrefab != null)
            {
                GameObject anchorObj = Instantiate(gameWorldAnchorPrefab, currentPlacementPose.position, currentPlacementPose.rotation);
                spawnedAnchor = anchorObj.transform;
            }
            else
            {
                GameObject anchorObj = new GameObject("[AR GameWorld Anchor]");
                anchorObj.transform.SetPositionAndRotation(currentPlacementPose.position, currentPlacementPose.rotation);
                spawnedAnchor = anchorObj.transform;
            }

            // Hide the placement reticle
            if (activeReticle != null)
            {
                activeReticle.SetActive(false);
            }

            // Stop detecting new planes and hide planes to maximize performance
            SetPlaneDetectionEnabled(false);

            // Start gameplay loop
            GameManager.Instance.StartGame(spawnedAnchor);
        }

        private void SetPlaneDetectionEnabled(bool isEnabled)
        {
            if (arPlaneManager != null)
            {
                arPlaneManager.enabled = isEnabled;

                // Also hide all existing plane gameObjects if disabling
                if (!isEnabled)
                {
                    foreach (var plane in arPlaneManager.trackables)
                    {
                        if (plane != null && plane.gameObject != null)
                        {
                            plane.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        private void ApplyNameToTracker(GameObject tracker)
        {
            // Attach 3D text badge with student's full name
            var nameBadge = tracker.GetComponentInChildren<TextMesh>();
            if (nameBadge == null)
            {
                GameObject textObj = new GameObject("TrackerStudentName");
                textObj.transform.SetParent(tracker.transform);
                textObj.transform.localPosition = new Vector3(0, 0.05f, 0.25f);
                textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                nameBadge = textObj.AddComponent<TextMesh>();
                nameBadge.characterSize = 0.04f;
                nameBadge.fontSize = 48;
                nameBadge.alignment = TextAlignment.Center;
                nameBadge.anchor = TextAnchor.MiddleCenter;
                nameBadge.color = Color.cyan;
            }
            nameBadge.text = studentName;
        }

        private bool IsPointerOverUI(int fingerId)
        {
            if (EventSystem.current == null) return false;
            if (fingerId >= 0) return EventSystem.current.IsPointerOverGameObject(fingerId);
            return EventSystem.current.IsPointerOverGameObject();
        }

        public void ResetPlacement()
        {
            hasPlacedGameWorld = false;
            if (spawnedAnchor != null)
            {
                Destroy(spawnedAnchor.gameObject);
                spawnedAnchor = null;
            }
            SetPlaneDetectionEnabled(true);
        }
    }
}
