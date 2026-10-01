using UnityEngine;

namespace SurvivalShooter.Audio
{
    /// <summary>
    /// Singleton Audio Manager handling all gameplay and UI acoustic events.
    /// Uses dedicated AudioSource channels to prevent clipping and eliminate component duplication.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources (Shared Channels)")]
        [SerializeField] private AudioSource playerChannel;
        [SerializeField] private AudioSource enemyChannel;
        [SerializeField] private AudioSource uiChannel;
        [SerializeField] private AudioSource ambientChannel;

        [Header("Mandatory Sound Clips")]
        [SerializeField] private AudioClip playerShootClip;
        [SerializeField] private AudioClip playerDeathClip;
        [SerializeField] private AudioClip enemySpawnClip;
        [SerializeField] private AudioClip enemyShootClip;
        [SerializeField] private AudioClip enemyDamageClip; // Melee attack / player hit

        [Header("Supplementary Audio Clips")]
        [SerializeField] private AudioClip enemyDeathClip;
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip gameOverDefeatClip;
        [SerializeField] private AudioClip gameOverVictoryClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioChannels();
        }

        private void EnsureAudioChannels()
        {
            if (playerChannel == null) playerChannel = CreateChannel("PlayerChannel");
            if (enemyChannel == null) enemyChannel = CreateChannel("EnemyChannel");
            if (uiChannel == null) uiChannel = CreateChannel("UIChannel");
            if (ambientChannel == null)
            {
                ambientChannel = CreateChannel("AmbientChannel");
                ambientChannel.loop = true;
                ambientChannel.volume = 0.35f;
            }
        }

        private AudioSource CreateChannel(string channelName)
        {
            GameObject channelObj = new GameObject(channelName);
            channelObj.transform.SetParent(transform);
            AudioSource source = channelObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D clean mix for mobile AR
            return source;
        }

        // ================= Mandatory Sound Events =================

        public void PlayPlayerShoot()
        {
            PlayWithVariation(playerChannel, playerShootClip, 0.95f, 1.05f);
        }

        public void PlayPlayerDeath()
        {
            if (playerDeathClip != null && playerChannel != null)
            {
                playerChannel.pitch = 1.0f;
                playerChannel.PlayOneShot(playerDeathClip, 1.0f);
            }
        }

        public void PlayEnemySpawn()
        {
            PlayWithVariation(enemyChannel, enemySpawnClip, 0.9f, 1.1f, 0.8f);
        }

        public void PlayEnemyShoot()
        {
            PlayWithVariation(enemyChannel, enemyShootClip, 0.92f, 1.08f, 0.85f);
        }

        public void PlayEnemyDamage()
        {
            PlayWithVariation(enemyChannel, enemyDamageClip, 0.95f, 1.05f, 0.9f);
        }

        // ================= Supplementary Sound Events =================

        public void PlayEnemyDeath()
        {
            PlayWithVariation(enemyChannel, enemyDeathClip, 0.9f, 1.1f, 0.85f);
        }

        public void PlayUIClick()
        {
            if (uiClickClip != null && uiChannel != null)
            {
                uiChannel.pitch = 1.0f;
                uiChannel.PlayOneShot(uiClickClip, 0.7f);
            }
        }

        public void PlayGameOver(bool isVictory)
        {
            AudioClip clip = isVictory ? gameOverVictoryClip : gameOverDefeatClip;
            if (clip != null && uiChannel != null)
            {
                uiChannel.pitch = 1.0f;
                uiChannel.PlayOneShot(clip, 1.0f);
            }
        }

        private void PlayWithVariation(AudioSource source, AudioClip clip, float minPitch, float maxPitch, float volume = 1f)
        {
            if (source == null || clip == null) return;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(clip, volume);
        }

        public void SetClips(
            AudioClip playerShoot, AudioClip playerDeath,
            AudioClip enemySpawn, AudioClip enemyShoot, AudioClip enemyDamage,
            AudioClip enemyDeath, AudioClip uiClick, AudioClip gameOver, AudioClip victory)
        {
            playerShootClip = playerShoot;
            playerDeathClip = playerDeath;
            enemySpawnClip = enemySpawn;
            enemyShootClip = enemyShoot;
            enemyDamageClip = enemyDamage;
            enemyDeathClip = enemyDeath;
            uiClickClip = uiClick;
            gameOverDefeatClip = gameOver;
            gameOverVictoryClip = victory;
        }
    }
}
