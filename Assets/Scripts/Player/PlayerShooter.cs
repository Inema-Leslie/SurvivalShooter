using UnityEngine;
using UnityEngine.EventSystems;
using SurvivalShooter.Core;
using SurvivalShooter.Audio;
using SurvivalShooter.Pooling;

namespace SurvivalShooter.Player
{
    /// <summary>
    /// Handles First-Person AR shooting mechanics.
    /// Uses Object Pooling for all projectiles (Mandatory requirement).
    /// Supports touch input on mobile and mouse/keyboard in Editor.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] private float fireRate = 0.22f;
        [SerializeField] private float bulletSpeed = 28f;
        [SerializeField] private float bulletDamage = 25f;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private ParticleSystem muzzleFlash;

        [Header("Weapon Visual Bob/Recoil")]
        [SerializeField] private Transform weaponTransform;
        [SerializeField] private float recoilKick = 0.04f;
        [SerializeField] private float recoilRecoverySpeed = 10f;

        private Camera playerCamera;
        private float nextFireTime;
        private Vector3 originalWeaponPos;

        private void Awake()
        {
            playerCamera = GetComponent<Camera>();
            if (playerCamera == null) playerCamera = Camera.main;

            if (weaponTransform != null)
            {
                originalWeaponPos = weaponTransform.localPosition;
            }
        }

        private void Update()
        {
            // Recoil recovery
            if (weaponTransform != null)
            {
                weaponTransform.localPosition = Vector3.Lerp(
                    weaponTransform.localPosition,
                    originalWeaponPos,
                    Time.deltaTime * recoilRecoverySpeed
                );
            }

            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            // Editor / PC testing
            if (Input.GetMouseButtonDown(0))
            {
                if (!IsPointerOverUI())
                {
                    Shoot();
                }
            }

            // Mobile touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (!IsTouchOverUI(touch))
                    {
                        Shoot();
                    }
                }
            }
        }

        /// <summary>
        /// Public method callable by in-game UI Shoot Button.
        /// </summary>
        public void TriggerShootFromUI()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                Shoot();
            }
        }

        public void Shoot()
        {
            if (Time.time < nextFireTime) return;
            nextFireTime = Time.time + fireRate;

            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera == null) return;

            // Determine shoot direction through center of screen
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, 50f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(40f);
            }

            Vector3 spawnPos = muzzlePoint != null ? muzzlePoint.position : ray.origin + ray.direction * 0.3f;
            Vector3 shootDir = (targetPoint - spawnPos).normalized;
            Quaternion spawnRot = Quaternion.LookRotation(shootDir);

            // MANDATORY: Use Object Pooling for shooting
            if (ObjectPoolManager.Instance != null)
            {
                GameObject bulletObj = ObjectPoolManager.Instance.SpawnFromPool(
                    ObjectPoolManager.TAG_PLAYER_PROJECTILE,
                    spawnPos,
                    spawnRot
                );

                if (bulletObj != null)
                {
                    Projectile proj = bulletObj.GetComponent<Projectile>();
                    if (proj != null)
                    {
                        proj.Configure(bulletSpeed, bulletDamage, true, ObjectPoolManager.TAG_PLAYER_PROJECTILE);
                    }
                }
            }

            // Muzzle flash particle
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }

            // Weapon kick animation
            if (weaponTransform != null)
            {
                weaponTransform.localPosition = originalWeaponPos - Vector3.forward * recoilKick;
            }

            // MANDATORY: Play Player Shoot Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPlayerShoot();
            }
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private bool IsTouchOverUI(Touch touch)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId);
        }
    }
}
