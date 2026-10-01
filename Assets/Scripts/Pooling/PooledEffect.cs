using UnityEngine;

namespace SurvivalShooter.Pooling
{
    /// <summary>
    /// Self-recycling pooled particle effect or visual flare.
    /// </summary>
    public class PooledEffect : MonoBehaviour, IPooledObject
    {
        [SerializeField] private string poolTag = ObjectPoolManager.TAG_HIT_EFFECT;
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private ParticleSystem particles;

        private float timer;

        public void SetTag(string tag)
        {
            poolTag = tag;
        }

        public void OnObjectSpawn()
        {
            timer = 0f;
            if (particles == null) particles = GetComponent<ParticleSystem>();
            if (particles != null)
            {
                particles.Clear();
                particles.Play();
            }
        }

        public void ReturnToPool()
        {
            if (particles != null)
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
            }
        }
    }
}
