namespace SurvivalShooter.Core
{
    public enum GameState
    {
        MainMenu,        // Title, difficulty, leaderboard access
        PlaneScanning,   // AR plane search active, reticle visualizer
        PlacementReady,  // Plane detected, awaiting user tap
        Playing,         // In combat: enemies spawning, timer ticking, shooting active
        GameOver,        // Player dead or survived time limit, summary displayed
        Leaderboard      // Viewing top scores
    }

    public enum EnemyType
    {
        Melee,
        Shooter
    }

    public enum DifficultyLevel
    {
        Normal,
        Hard,
        Nightmare
    }
}
