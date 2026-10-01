namespace SurvivalShooter.Pooling
{
    /// <summary>
    /// Contract for pooled entities requiring lifecycle hooks upon acquisition and recycle.
    /// Eliminates GC overhead and avoids Instantiate/Destroy during gameplay.
    /// </summary>
    public interface IPooledObject
    {
        void OnObjectSpawn();
        void ReturnToPool();
    }
}
