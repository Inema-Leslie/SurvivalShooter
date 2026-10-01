using System.Collections.Generic;
using UnityEngine;
using SurvivalShooter.Core;
using SurvivalShooter.Audio;

namespace SurvivalShooter.Enemies
{
    /// <summary>
    /// Factory and Spawner for creating and managing active enemy instances.
    /// Handles difficulty wave progression and plane-anchored positioning.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject meleePrefab;
        [SerializeField] private GameObject shooterPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float baseSpawnInterval = 3.5f;
        [SerializeField] private float minSpawnDistance = 2.0f;
        [SerializeField] private float maxSpawnDistance = 5.0f;
        [SerializeField] private int maxSimultaneousEnemies = 8;
        [Range(0f, 1f)]
        [SerializeField] private float shooterSpawnRatio = 0.35f;

        private readonly List<EnemyBase> activeEnemies = new List<EnemyBase>();
        private float spawnTimer;
        private Transform anchorTransform;
        private bool isSpawningActive = false;

        private float currentSpawnInterval;
        private float difficultySpeedMult = 1.0f;
        private float difficultyHealthMult = 1.0f;
        private float difficultyDamageMult = 1.0f;

        public int ActiveEnemyCount => activeEnemies.Count;

        public void SetAnchor(Transform anchor)
        {
            anchorTransform = anchor;
        }

        public void StartSpawning(DifficultyLevel difficulty)
        {
            ApplyDifficulty(difficulty);
            activeEnemies.Clear();
            spawnTimer = 1.0f; // Brief grace period before first spawn
            isSpawningActive = true;
        }

        public void StopSpawning()
        {
            isSpawningActive = false;
        }

        private void ApplyDifficulty(DifficultyLevel difficulty)
        {
            switch (difficulty)
            {
                case DifficultyLevel.Normal:
                    currentSpawnInterval = baseSpawnInterval;
                    difficultySpeedMult = 1.0f;
                    difficultyHealthMult = 1.0f;
                    difficultyDamageMult = 1.0f;
                    maxSimultaneousEnemies = 7;
                    shooterSpawnRatio = 0.3f;
                    break;

                case DifficultyLevel.Hard:
                    currentSpawnInterval = baseSpawnInterval * 0.7f;
                    difficultySpeedMult = 1.25f;
                    difficultyHealthMult = 1.2f;
                    difficultyDamageMult = 1.3f;
                    maxSimultaneousEnemies = 10;
                    shooterSpawnRatio = 0.45f;
                    break;

                case DifficultyLevel.Nightmare:
                    currentSpawnInterval = baseSpawnInterval * 0.45f;
                    difficultySpeedMult = 1.5f;
                    difficultyHealthMult = 1.4f;
                    difficultyDamageMult = 1.6f;
                    maxSimultaneousEnemies = 14;
                    shooterSpawnRatio = 0.5f;
                    break;
            }
        }

        private void Update()
        {
            if (!isSpawningActive) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = currentSpawnInterval;
                if (activeEnemies.Count < maxSimultaneousEnemies)
                {
                    SpawnRandomEnemy();
                }
            }
        }

        private void SpawnRandomEnemy()
        {
            Vector3 spawnPosition = CalculateSpawnPosition();
            bool spawnShooter = (Random.value < shooterSpawnRatio && shooterPrefab != null);
            GameObject prefabToSpawn = spawnShooter ? shooterPrefab : meleePrefab;

            if (prefabToSpawn == null)
            {
                // Fallback to whichever is available
                prefabToSpawn = meleePrefab != null ? meleePrefab : shooterPrefab;
                if (prefabToSpawn == null) return;
            }

            GameObject enemyObj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                enemy.ApplyDifficultyMultipliers(difficultySpeedMult, difficultyHealthMult, difficultyDamageMult);
                enemy.OnEnemyDied += HandleEnemyDied;
                activeEnemies.Add(enemy);

                // Play mandatory Enemy Spawn Sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnemySpawn();
                }
            }
        }

        private Vector3 CalculateSpawnPosition()
        {
            Vector3 center = anchorTransform != null ? anchorTransform.position : Vector3.zero;
            if (Camera.main != null)
            {
                center = Camera.main.transform.position;
            }

            // Random angle on horizontal plane
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

            float x = center.x + Mathf.Cos(angle) * distance;
            float z = center.z + Mathf.Sin(angle) * distance;
            float y = anchorTransform != null ? anchorTransform.position.y : 0f;

            return new Vector3(x, y, z);
        }

        private void HandleEnemyDied(EnemyBase enemy)
        {
            if (enemy != null)
            {
                enemy.OnEnemyDied -= HandleEnemyDied;
                activeEnemies.Remove(enemy);
            }
        }

        /// <summary>
        /// Cleans up all active enemies instantly when game ends or restarts.
        /// </summary>
        public void WipeAllEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] != null)
                {
                    activeEnemies[i].OnEnemyDied -= HandleEnemyDied;
                    activeEnemies[i].Wipe();
                }
            }
            activeEnemies.Clear();
        }
    }
}
