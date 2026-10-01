# AR Survival Shooter

An Augmented Reality mobile survival combat game built with **Unity 6** and **AR Foundation**.

- **Target Platform:** Android & iOS (AR Foundation / ARCore / ARKit)
- **Player Perspective:** First-Person Shooter (FPS)


---

##  Game Overview

The player uses their mobile device to scan physical horizontal planes in their environment. Upon detecting a surface, a **Custom Plane Tracker** displaying **"INEMA AMANDA LESLIE - AR PLANE TRACKER"** appears. Tapping on the plane anchors the game arena and initiates the survival combat loop:
- **Melee Enemies**  swarm toward the player to deal melee damage. They have 50 HP (destroyed in 2 player shots).
- **Shooter Enemies**  advance to combat distance, stop, and fire projectiles. They have 125 HP (destroyed in 5 player shots).
- The player must aim their device, tap the screen or press the **FIRE** button to shoot pooled projectiles, and survive until the timer expires.
- Final score, kills, and survival time are automatically logged to a **Local Leaderboard** displaying the **latest 5 gameplay sessions**.

---

## Key Features & Architectural Compliance

| Requirement | Implementation Details |
| :--- | :--- |
| **AR Foundation** | Horizontal plane detection, raycasting, and anchor placement via `ARPlacementManager`. Single placement enforced (subsequent taps ignored). Plane detection auto-disabled upon placement to optimize battery and tracking. |
| **Custom Plane Tracker** | Replaces default visualizer with custom textured cyber mesh displaying **"Inema Amanda Leslie"** in prominent 3D typography (`CustomPlaneVisualizer.cs`). Shows only when plane is detected. |
| **Player System (FPS)** | AR camera viewport with center aiming crosshair, `PlayerShooter` weapon system, `PlayerHealth` (100 HP) with screen damage flash, and health bar. |
| **Object Pooling (Mandatory)** | `ObjectPoolManager` pre-instantiates reusable pools for player bullets, enemy bullets, hit sparks, and death VFX. **Zero runtime Instantiate or Destroy during shooting**. |
| **Enemy OOP Hierarchy** | Abstract `EnemyBase` (`IDamageable`) extended by `MeleeEnemy` and `ShooterEnemy`. Implements Encapsulation, Abstraction, Inheritance, and Polymorphism. |
| **Sound Design (Mandatory)** | Centralized `AudioManager` with dedicated AudioSource channels. All 5 mandatory sound events implemented: Player Shoot, Player Death, Enemy Spawn, Enemy Shoot, and Enemy Damage (Melee attack). Includes supplementary sounds (death rumble, UI click, victory/defeat). |
| **UI System** | Complete state machine UI (`UIManager.cs`): Main Menu with custom artwork background, "Survival shooter" title, Start button, Leaderboard button, Placement Guide, In-Game Combat HUD, and End Game Summary Modal. |
| **Local Leaderboard** | `LeaderboardManager.cs` persists session history to local JSON and PlayerPrefs, strictly displaying the **latest 5 sessions** (`LeaderboardUI.cs`). |
| **Difficulty Presets (Bonus)** | Normal (60s), Hard (75s), and Nightmare (90s) with dynamically scaled spawn rates, enemy speed, and health multipliers. |

---

## Project Structure

```
Assets/
├── Audio/                    
│   ├── PlayerShoot.wav
│   ├── PlayerDeath.wav
│   ├── EnemySpawn.wav
│   ├── EnemyShoot.wav
│   ├── EnemyDamage.wav
│   ├── EnemyDeath.wav
│   ├── UIClick.wav
│   ├── GameOverDefeat.wav
│   └── GameOverVictory.wav
├── Prefabs/                  
│   ├── MeleeEnemy.prefab
│   ├── ShooterEnemy.prefab
│   ├── PlayerBullet.prefab
│   ├── EnemyBullet.prefab
│   ├── CustomPlaneTracker.prefab
│   └── HitEffect.prefab
├── Scenes/
│   └── MainARSurvivalShooter.unity
└── Scripts/
    ├── AR/
    │   ├── ARPlacementManager.cs
    │   └── CustomPlaneVisualizer.cs
    ├── Audio/
    │   └── AudioManager.cs
    ├── Core/
    │   ├── Enums.cs
    │   ├── IDamageable.cs
    │   ├── GameManager.cs
    │   └── RuntimeGameBootstrapper.cs
    ├── Enemies/
    │   ├── EnemyBase.cs
    │   ├── MeleeEnemy.cs
    │   ├── ShooterEnemy.cs
    │   └── EnemySpawner.cs
    ├── Leaderboard/
    │   └── LeaderboardManager.cs
    ├── Player/
    │   ├── PlayerHealth.cs
    │   └── PlayerShooter.cs
    ├── Pooling/
    │   ├── IPooledObject.cs
    │   ├── ObjectPoolManager.cs
    │   ├── Projectile.cs
    │   └── PooledEffect.cs
    ├── UI/
    │   ├── UIManager.cs
    │   └── LeaderboardUI.cs
    ├── Utils/
    │   ├── ProceduralAudioGenerator.cs
    │   └── ProceduralModelBuilder.cs
    └── Editor/
        └── GameSceneSetupWindow.cs
```

---

## How to Run in Unity Editor

1. Open this project in **Unity 6 (6000.4.5f1)**.
2. In the Unity menu, select:  
   **`Survival Shooter -> Setup Complete Game Scene`**.  
   *(This one-click command generates all WAV audio files, builds all prefabs, and configures the scene `MainARSurvivalShooter.unity`)*.
3. Open `Assets/Scenes/MainARSurvivalShooter.unity`.
4. Press **Play**:
   - The game includes an **Editor Simulation fallback** for rapid testing without a phone:
   - Click **START MISSION** on the Start Menu.
   - Point your camera towards the floor and click to place the game world anchor.
   - Aim with your mouse and click / press **FIRE** to shoot enemies.

---

## Mobile Build Instructions (Android ARCore)

1. Open **File -> Build Settings...**
2. Switch platform to **Android**.
3. Under **Project Settings -> XR Plug-in Management**, enable **Google ARCore**.
4. In **Project Settings -> Player -> Other Settings**:
   - **Minimum API Level**: Android 7.0 'Nougat' (API Level 24) or higher.
   - **Scripting Backend**: `IL2CPP`.
   - **Target Architectures**: Check `ARM64`.
5. Connect your ARCore-compatible Android phone with USB Debugging enabled.
6. Click **Build and Run**.
