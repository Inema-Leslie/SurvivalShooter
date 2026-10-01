using UnityEngine;

namespace SurvivalShooter.Core
{
    /// <summary>
    /// Abstraction for any entity that can take damage and report vital state.
    /// Used by PlayerHealth, MeleeEnemy, and ShooterEnemy.
    /// </summary>
    public interface IDamageable
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsAlive { get; }

        void TakeDamage(float amount, Vector3 hitPoint = default);
    }
}
