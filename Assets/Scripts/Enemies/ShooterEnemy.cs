using UnityEngine;
using SurvivalShooter.Core;
using SurvivalShooter.Audio;
using SurvivalShooter.Pooling;

namespace SurvivalShooter.Enemies
{
    /// <summary>
    /// Concrete Shooter Enemy (OOP Inheritance & Polymorphism).
    /// Maintains standoff distance, hovers/aims, and fires pooled projectiles at player.
    /// Higher health / ranged archetype. Destroyed in 5 player shots (125 HP).
    /// </summary>
    public class ShooterEnemy : EnemyBase
    {
        [Header("Shooter Specifics")]
        [SerializeField] private float preferredCombatDistance = 2.8f;
        [SerializeField] private float combatDistanceTolerance = 0.5f;
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private float projectileSpeed = 12f;
        [SerializeField] private Transform rotatingCore;

        protected override void Awake()
        {
            enemyType = EnemyType.Shooter;
            maxHealth = 125f;    // Takes 5 player bullets (25 dmg each)
            damageAmount = 15f;  // Bullet damage
            moveSpeed = 0.9f;    // Slower, methodical positioning
            attackCooldown = 2.2f;
            scoreValue = 250;    // Higher reward

            base.Awake();
        }

        protected override void HandleMovement()
        {
            if (playerTransform == null) return;

            Vector3 targetPos = playerTransform.position;
            float distance = Vector3.Distance(transform.position, targetPos);

            LookAtTarget(targetPos, 5f);

            // Standoff positioning: move closer if too far, retreat if player gets too close
            if (distance > preferredCombatDistance + combatDistanceTolerance)
            {
                // Advance
                Vector3 moveDir = (targetPos - transform.position).normalized;
                moveDir.y = 0;
                transform.position += moveDir * (moveSpeed * Time.deltaTime);
            }
            else if (distance < preferredCombatDistance - combatDistanceTolerance)
            {
                // Back up
                Vector3 retreatDir = (transform.position - targetPos).normalized;
                retreatDir.y = 0;
                transform.position += retreatDir * (moveSpeed * 0.7f * Time.deltaTime);
            }

            // Procedural visual hover and rotating weapon core
            if (rotatingCore != null)
            {
                rotatingCore.Rotate(Vector3.up, 90f * Time.deltaTime, Space.Self);
            }
        }

        protected override void CheckAttackCondition()
        {
            if (playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            // Shooter can attack from medium/long range
            if (distance <= preferredCombatDistance + 1.5f && Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }

        protected override void Attack()
        {
            lastAttackTime = Time.time;

            Vector3 spawnPos = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up * 0.4f + transform.forward * 0.3f;
            Vector3 aimDir = (playerTransform.position - spawnPos).normalized;
            Quaternion spawnRot = Quaternion.LookRotation(aimDir);

            // Use Object Pooling pattern for projectile
            if (ObjectPoolManager.Instance != null)
            {
                GameObject bulletObj = ObjectPoolManager.Instance.SpawnFromPool(
                    ObjectPoolManager.TAG_ENEMY_PROJECTILE,
                    spawnPos,
                    spawnRot
                );

                if (bulletObj != null)
                {
                    Projectile proj = bulletObj.GetComponent<Projectile>();
                    if (proj != null)
                    {
                        proj.Configure(projectileSpeed, damageAmount, false, ObjectPoolManager.TAG_ENEMY_PROJECTILE);
                    }
                }
            }

            // Play mandatory Enemy Shoot Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyShoot();
            }
        }
    }
}
