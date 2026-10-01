using System;
using System.IO;
using UnityEngine;

namespace SurvivalShooter.Utils
{
    /// <summary>
    /// Generates high quality procedural audio clips for all mandatory gameplay events.
    /// Synthesizes waveforms directly so the game sounds great out of the box with zero external dependencies.
    /// Can also export them as standard 16-bit PCM .WAV files.
    /// </summary>
    public static class ProceduralAudioGenerator
    {
        private const int SAMPLE_RATE = 44100;

        public static AudioClip CreatePlayerShootClip()
        {
            float duration = 0.18f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                // Pitch drop: 880Hz down to 180Hz
                float freq = Mathf.Lerp(880f, 180f, progress * progress);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                // Punchy square overtone
                float square = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * (freq * 0.5f) * t)) * 0.25f;
                // Exponential decay envelope
                float env = Mathf.Exp(-14f * progress);
                data[i] = Mathf.Clamp((wave * 0.7f + square) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("PlayerShoot", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreatePlayerDeathClip()
        {
            float duration = 1.2f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float freq = Mathf.Lerp(220f, 40f, progress);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.35f * Mathf.Exp(-3f * progress);
                float env = Mathf.Exp(-3.5f * progress);
                data[i] = Mathf.Clamp((wave + noise) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("PlayerDeath", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateEnemySpawnClip()
        {
            float duration = 0.35f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                // Cyber warp frequency rise
                float freq = Mathf.Lerp(200f, 750f, Mathf.Sqrt(progress));
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float tremolo = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 30f * t);
                float env = Mathf.Sin(progress * Mathf.PI);
                data[i] = Mathf.Clamp(wave * tremolo * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("EnemySpawn", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateEnemyShootClip()
        {
            float duration = 0.22f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                // Laser chirp: 650Hz down to 220Hz
                float freq = Mathf.Lerp(650f, 220f, progress);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float env = Mathf.Exp(-12f * progress);
                data[i] = Mathf.Clamp(wave * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("EnemyShoot", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateEnemyDamageClip()
        {
            float duration = 0.25f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                // Melee strike / bite crunch
                float freq = Mathf.Lerp(140f, 60f, progress);
                float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.6f;
                float env = Mathf.Exp(-14f * progress);
                data[i] = Mathf.Clamp((wave * 0.5f + noise * 0.5f) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("EnemyDamage", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateEnemyDeathClip()
        {
            float duration = 0.45f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float noise = (UnityEngine.Random.value * 2f - 1f);
                float sub = Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.6f;
                float env = Mathf.Exp(-7f * progress);
                data[i] = Mathf.Clamp((noise * 0.6f + sub) * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("EnemyDeath", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateUIClickClip()
        {
            float duration = 0.08f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float wave = Mathf.Sin(2f * Mathf.PI * 1200f * t);
                float env = Mathf.Exp(-35f * progress);
                data[i] = Mathf.Clamp(wave * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("UIClick", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static AudioClip CreateGameOverClip(bool isVictory)
        {
            float duration = 1.4f;
            int samples = (int)(SAMPLE_RATE * duration);
            float[] data = new float[samples];

            float[] chord = isVictory ? new float[] { 523.25f, 659.25f, 783.99f, 1046.50f } // C Major
                                      : new float[] { 311.13f, 293.66f, 246.94f, 196.00f }; // Dramatic minor

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SAMPLE_RATE;
                float progress = (float)i / samples;
                float sum = 0f;
                foreach (var f in chord)
                {
                    sum += Mathf.Sin(2f * Mathf.PI * f * t);
                }
                sum /= chord.Length;
                float env = Mathf.Exp(-2.5f * progress);
                data[i] = Mathf.Clamp(sum * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(isVictory ? "Victory" : "Defeat", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// Encodes an AudioClip float array into standard 16-bit uncompressed WAV bytes.
        /// </summary>
        public static byte[] EncodeToWav(AudioClip clip)
        {
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                int sampleCount = samples.Length;
                int byteRate = clip.frequency * clip.channels * 2;

                // RIFF header
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + sampleCount * 2);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

                // fmt chunk
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16); // Chunk size
                writer.Write((short)1); // PCM
                writer.Write((short)clip.channels);
                writer.Write(clip.frequency);
                writer.Write(byteRate);
                writer.Write((short)(clip.channels * 2)); // Block align
                writer.Write((short)16); // Bits per sample

                // data chunk
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(sampleCount * 2);

                for (int i = 0; i < sampleCount; i++)
                {
                    short val = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767f);
                    writer.Write(val);
                }

                return stream.ToArray();
            }
        }
    }
}
