using System;
using UnityEngine;
using SurvivalShooter.Core;
using SurvivalShooter.Audio;

namespace SurvivalShooter.Player
{
    /// <summary>
    /// Encapsulates player health, damage reactions, and death events.
    /// Implements IDamageable interface.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float damageCooldown = 0.4f;

        private float lastDamageTime;
        private bool isAlive = true;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => isAlive;

        public static event Action<float, float> OnHealthChanged; // (current, max)
        public static event Action OnPlayerDamaged;
        public static event Action OnPlayerDied;

        private void Awake()
        {
            ResetHealth();
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isAlive = true;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(float amount, Vector3 hitPoint = default)
        {
            if (!isAlive || Time.time < lastDamageTime + damageCooldown) return;
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            lastDamageTime = Time.time;
            currentHealth = Mathf.Max(0f, currentHealth - amount);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnPlayerDamaged?.Invoke();

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (!isAlive) return;
            isAlive = false;

            // Play mandatory Player Death Sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPlayerDeath();
            }

            OnPlayerDied?.Invoke();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(false); // Defeat
            }
        }
    }
}
