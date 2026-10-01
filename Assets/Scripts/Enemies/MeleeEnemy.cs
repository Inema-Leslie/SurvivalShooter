using UnityEngine;
using SurvivalShooter.Core;
using SurvivalShooter.Audio;

namespace SurvivalShooter.Enemies
{
    /// <summary>
    /// Concrete Melee Enemy (OOP Inheritance & Polymorphism).
    /// Closes distance aggressively and executes proximity melee bites/slashes.
    /// Low health / high mobility archetype. Destroyed in 2 player shots (50 HP).
    /// </summary>
    public class MeleeEnemy : EnemyBase
    {
        [Header("Melee Specifics")]
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float lungeSpeedMultiplier = 2.0f;
        [SerializeField] private Transform animatedModel;

        private bool isLunging = false;
        private float originalY;

        protected override void Awake()
        {
            enemyType = EnemyType.Melee;
            maxHealth = 50f;     // Takes 2 player bullets (25 dmg each)
            damageAmount = 20f;  // Melee damage
            moveSpeed = 1.6f;    // Faster than shooter
            attackCooldown = 1.2f;
            scoreValue = 100;

            base.Awake();
            originalY = transform.position.y;
        }

        protected override void HandleMovement()
        {
            if (playerTransform == null) return;

            Vector3 targetPos = playerTransform.position;
            float distance = Vector3.Distance(transform.position, targetPos);

            LookAtTarget(targetPos, 8f);

            // If outside attack range, chase player
            if (distance > attackRange)
            {
                float currentSpeed = isLunging ? moveSpeed * lungeSpeedMultiplier : moveSpeed;
                Vector3 moveDir = (targetPos - transform.position).normalized;
                moveDir.y = 0; // Stay level with AR plane

                transform.position += moveDir * (currentSpeed * Time.deltaTime);

                // Gentle procedural walking bob
                if (animatedModel != null)
                {
                    float bob = Mathf.Sin(Time.time * 12f) * 0.05f;
                    animatedModel.localPosition = new Vector3(0, bob, 0);
                }
            }
        }

        protected override void CheckAttackCondition()
        {
            if (playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }

        protected override void Attack()
        {
            lastAttackTime = Time.time;

            // Deal damage to player via IDamageable
            IDamageable playerDamageable = playerTransform.GetComponentInParent<IDamageable>();
            if (playerDamageable != null && playerDamageable.IsAlive)
            {
                playerDamageable.TakeDamage(damageAmount, transform.position);

                // Play mandatory Melee Attack / Enemy Damage Sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnemyDamage();
                }
            }
        }
    }
}
