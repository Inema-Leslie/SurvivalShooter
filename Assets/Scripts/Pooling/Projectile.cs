using UnityEngine;
using SurvivalShooter.Core;

namespace SurvivalShooter.Pooling
{
    /// <summary>
    /// Pooled Projectile used by both Player and ShooterEnemy.
    /// Implements IPooledObject for clean lifecycle resetting without GC allocations.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour, IPooledObject
    {
        [Header("Projectile Properties")]
        [SerializeField] private float speed = 25f;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float maxLifetime = 3f;
        [SerializeField] private string poolTag = ObjectPoolManager.TAG_PLAYER_PROJECTILE;
        [SerializeField] private bool isPlayerProjectile = true;

        [Header("Visual Components")]
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private Rigidbody rb;

        private float currentLifetime;
        private bool hasHit;

        public void Configure(float customSpeed, float customDamage, bool fromPlayer, string tag)
        {
            speed = customSpeed;
            damage = customDamage;
            isPlayerProjectile = fromPlayer;
            poolTag = tag;
        }

        public void OnObjectSpawn()
        {
            currentLifetime = 0f;
            hasHit = false;

            if (rb == null) rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * speed;
            }

            if (trailRenderer == null) trailRenderer = GetComponentInChildren<TrailRenderer>();
            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.emitting = true;
            }
        }

        public void ReturnToPool()
        {
            hasHit = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (trailRenderer != null)
            {
                trailRenderer.emitting = false;
                trailRenderer.Clear();
            }
        }

        private void Update()
        {
            // Move if Rigidbody is kinematic or not driven by physics
            if (rb == null || rb.isKinematic)
            {
                transform.position += transform.forward * (speed * Time.deltaTime);
            }

            currentLifetime += Time.deltaTime;
            if (currentLifetime >= maxLifetime && !hasHit)
            {
                hasHit = true;
                ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;

            // Collision filtering: Player projectiles hit enemies; Enemy projectiles hit player
            if (isPlayerProjectile)
            {
                if (other.CompareTag("Player")) return;

                IDamageable damageable = other.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    hasHit = true;
                    damageable.TakeDamage(damage, transform.position);
                    SpawnImpactEffect(transform.position);
                    ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
                }
                else if (!other.isTrigger)
                {
                    hasHit = true;
                    SpawnImpactEffect(transform.position);
                    ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
                }
            }
            else // Enemy projectile
            {
                if (other.CompareTag("Enemy")) return;

                IDamageable playerHealth = other.GetComponentInParent<IDamageable>();
                if (playerHealth != null && playerHealth.IsAlive)
                {
                    hasHit = true;
                    playerHealth.TakeDamage(damage, transform.position);
                    SpawnImpactEffect(transform.position);
                    ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
                }
                else if (!other.isTrigger)
                {
                    hasHit = true;
                    SpawnImpactEffect(transform.position);
                    ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
                }
            }
        }

        private void SpawnImpactEffect(Vector3 pos)
        {
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.SpawnFromPool(ObjectPoolManager.TAG_HIT_EFFECT, pos, Quaternion.identity);
            }
        }
    }
}
