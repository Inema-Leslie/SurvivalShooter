using System;
using System.Collections;
using UnityEngine;
using SurvivalShooter.Core;
using SurvivalShooter.Audio;
using SurvivalShooter.Pooling;

namespace SurvivalShooter.Enemies
{
    /// <summary>
    /// Abstract base class for all enemies in the game.
    /// Demonstrates OOP Principles:
    /// - Abstraction: Abstract methods for movement and attack behavior.
    /// - Encapsulation: Protected/private state exposed via public properties.
    /// - Polymorphism: Virtual and abstract methods overridden by concrete enemies.
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] protected float maxHealth = 50f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 1.2f;
        [SerializeField] protected float damageAmount = 15f;
        [SerializeField] protected float attackCooldown = 1.5f;
        [SerializeField] protected int scoreValue = 100;
        [SerializeField] protected EnemyType enemyType = EnemyType.Melee;

        [Header("Visual Feedback")]
        [SerializeField] protected Renderer[] modelRenderers;
        [SerializeField] protected Color hitFlashColor = Color.white;
        [SerializeField] protected float flashDuration = 0.12f;

        protected Transform playerTransform;
        protected bool isAlive = true;
        protected float lastAttackTime;
        protected Coroutine flashCoroutine;
        private Color[] originalColors;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => isAlive;
        public int ScoreValue => scoreValue;
        public EnemyType Type => enemyType;

        public event Action<EnemyBase> OnEnemyDied;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            CacheRenderers();
        }

        protected virtual void Start()
        {
            FindPlayerTarget();
        }

        protected virtual void Update()
        {
            if (!isAlive || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (playerTransform == null)
            {
                FindPlayerTarget();
                if (playerTransform == null) return;
            }

            HandleMovement();
            CheckAttackCondition();
        }

        protected void FindPlayerTarget()
        {
            if (Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }
        }

        /// <summary>
        /// Polymorphic movement logic implemented by concrete derived classes.
        /// </summary>
        protected abstract void HandleMovement();

        /// <summary>
        /// Polymorphic attack logic implemented by concrete derived classes.
        /// </summary>
        protected abstract void Attack();

        /// <summary>
        /// Evaluates distance and attack cooldown to trigger Attack().
        /// </summary>
        protected abstract void CheckAttackCondition();

        /// <summary>
        /// Smoothly rotates the enemy around the Y-axis to face the target.
        /// </summary>
        protected void LookAtTarget(Vector3 targetPosition, float rotationSpeed = 6f)
        {
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0f; // Keep upright on horizontal plane
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        public virtual void TakeDamage(float amount, Vector3 hitPoint = default)
        {
            if (!isAlive) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);
            FlashHitFeedback();

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        protected virtual void FlashHitFeedback()
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            if (modelRenderers != null)
            {
                foreach (var rend in modelRenderers)
                {
                    if (rend != null && rend.material.HasProperty("_Color"))
                    {
                        rend.material.color = hitFlashColor;
                    }
                }
            }

            yield return new WaitForSeconds(flashDuration);

            ResetRendererColors();
        }

        private void CacheRenderers()
        {
            if (modelRenderers == null || modelRenderers.Length == 0)
            {
                modelRenderers = GetComponentsInChildren<Renderer>();
            }

            if (modelRenderers != null && modelRenderers.Length > 0)
            {
                originalColors = new Color[modelRenderers.Length];
                for (int i = 0; i < modelRenderers.Length; i++)
                {
                    if (modelRenderers[i] != null && modelRenderers[i].material.HasProperty("_Color"))
                    {
                        originalColors[i] = modelRenderers[i].material.color;
                    }
                    else
                    {
                        originalColors[i] = Color.gray;
                    }
                }
            }
        }

        private void ResetRendererColors()
        {
            if (modelRenderers != null && originalColors != null)
            {
                for (int i = 0; i < modelRenderers.Length; i++)
                {
                    if (modelRenderers[i] != null && i < originalColors.Length && modelRenderers[i].material.HasProperty("_Color"))
                    {
                        modelRenderers[i].material.color = originalColors[i];
                    }
                }
            }
        }

        protected virtual void Die()
        {
            if (!isAlive) return;
            isAlive = false;

            // Audio & particle feedback
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyDeath();
            }

            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.SpawnFromPool(ObjectPoolManager.TAG_ENEMY_DEATH_EFFECT, transform.position + Vector3.up * 0.3f, Quaternion.identity);
            }

            // Notify listeners (Spawner / GameManager)
            OnEnemyDied?.Invoke(this);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
                GameManager.Instance.RegisterEnemyDefeated();
            }

            Destroy(gameObject, 0.05f);
        }

        /// <summary>
        /// Cleans up the enemy instantly when the game ends or resets.
        /// </summary>
        public virtual void Wipe()
        {
            isAlive = false;
            Destroy(gameObject);
        }

        public void ApplyDifficultyMultipliers(float speedMult, float healthMult, float damageMult)
        {
            moveSpeed *= speedMult;
            maxHealth *= healthMult;
            currentHealth = maxHealth;
            damageAmount *= damageMult;
        }
    }
}
