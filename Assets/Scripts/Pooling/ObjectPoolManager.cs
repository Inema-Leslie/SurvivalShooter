using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalShooter.Pooling
{
    /// <summary>
    /// Singleton Object Pool Manager implementing the Object Pooling design pattern.
    /// Eliminates garbage collection spikes and runtime instantiation/destruction
    /// during gameplay shooting and particle effects.
    /// </summary>
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        [Serializable]
        public class PoolItem
        {
            public string tag;
            public GameObject prefab;
            public int size = 20;
            public bool expandable = true;
        }

        [Header("Pool Configurations")]
        [SerializeField] private List<PoolItem> pools = new List<PoolItem>();

        private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, PoolItem> poolSettings = new Dictionary<string, PoolItem>();
        private readonly Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>();

        public const string TAG_PLAYER_PROJECTILE = "PlayerProjectile";
        public const string TAG_ENEMY_PROJECTILE = "EnemyProjectile";
        public const string TAG_HIT_EFFECT = "HitEffect";
        public const string TAG_ENEMY_DEATH_EFFECT = "EnemyDeathEffect";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary.Clear();
            poolSettings.Clear();

            foreach (var pool in pools)
            {
                if (pool.prefab == null) continue;

                var poolQueue = new Queue<GameObject>();
                var parentGo = new GameObject($"[Pool] {pool.tag}");
                parentGo.transform.SetParent(transform);
                poolParents[pool.tag] = parentGo.transform;

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab, parentGo.transform);
                    obj.SetActive(false);
                    poolQueue.Enqueue(obj);
                }

                poolDictionary[pool.tag] = poolQueue;
                poolSettings[pool.tag] = pool;
            }
        }

        /// <summary>
        /// Registers a pool programmatically if not defined in Inspector.
        /// </summary>
        public void RegisterPool(string tag, GameObject prefab, int initialSize, bool expandable = true)
        {
            if (poolDictionary.ContainsKey(tag)) return;

            var poolItem = new PoolItem { tag = tag, prefab = prefab, size = initialSize, expandable = expandable };
            var poolQueue = new Queue<GameObject>();
            var parentGo = new GameObject($"[Pool] {tag}");
            parentGo.transform.SetParent(transform);
            poolParents[tag] = parentGo.transform;

            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab, parentGo.transform);
                obj.SetActive(false);
                poolQueue.Enqueue(obj);
            }

            poolDictionary[tag] = poolQueue;
            poolSettings[tag] = poolItem;
        }

        /// <summary>
        /// Retrieves an object from the pool, positioning it and invoking IPooledObject hooks.
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool with tag '{tag}' does not exist.");
                return null;
            }

            Queue<GameObject> queue = poolDictionary[tag];
            GameObject objToSpawn = null;

            if (queue.Count > 0)
            {
                objToSpawn = queue.Dequeue();
            }
            else if (poolSettings[tag].expandable)
            {
                Transform parent = poolParents[tag];
                objToSpawn = Instantiate(poolSettings[tag].prefab, parent);
            }
            else
            {
                Debug.LogWarning($"[ObjectPoolManager] Pool '{tag}' exhausted and not expandable.");
                return null;
            }

            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;
            objToSpawn.SetActive(true);

            IPooledObject pooledObj = objToSpawn.GetComponent<IPooledObject>();
            pooledObj?.OnObjectSpawn();

            return objToSpawn;
        }

        /// <summary>
        /// Recycles an object back to its designated pool, invoking cleanup hooks.
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPoolManager] No pool registered for tag '{tag}'. Destroying object.");
                Destroy(obj);
                return;
            }

            IPooledObject pooledObj = obj.GetComponent<IPooledObject>();
            pooledObj?.ReturnToPool();

            obj.SetActive(false);
            if (poolParents.TryGetValue(tag, out Transform parent))
            {
                obj.transform.SetParent(parent);
            }

            poolDictionary[tag].Enqueue(obj);
        }

        /// <summary>
        /// Recycles all active objects belonging to all pools (useful on game reset/end).
        /// </summary>
        public void RecycleAllActive()
        {
            foreach (var kvp in poolParents)
            {
                string tag = kvp.Key;
                Transform parent = kvp.Value;
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (child.gameObject.activeSelf)
                    {
                        ReturnToPool(tag, child.gameObject);
                    }
                }
            }
        }
    }
}
