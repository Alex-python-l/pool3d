// Real local AudioSource playback of original clips; no service/SDK substitute.
using System.Collections.Generic;
using UnityEngine;

namespace Pool3D.Offline
{
    public sealed class OfflineAudio : MonoBehaviour
    {
        private static OfflineAudio instance;
        public static OfflineAudio Instance() { return instance; }
        private AudioSource source;
        private readonly Dictionary<int, AudioClip> clips = new Dictionary<int, AudioClip>();
        private void Awake()
        {
            instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            string[] names = { "bump_ball", "bump_table", "lose", "pot", "shot_heavy", "shot_light", "shot_mid", "win" };
            int[] ids = { 0, 1, 5, 14, 15, 16, 17, 18 }; // Original SoundManager.Clips map.
            for (int i = 0; i < names.Length; i++)
            {
                AudioClip clip = Resources.Load<AudioClip>("OfflineAudio/" + names[i]);
                if (clip == null) Debug.LogWarning("Original audio unavailable: " + names[i]);
                else clips.Add(ids[i], clip);
            }
        }
        public void PlaySound2D(int id, float volume)
        {
            AudioClip clip;
            if (source != null && clips.TryGetValue(id, out clip)) source.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
        private void OnDestroy() { if (instance == this) instance = null; }
    }
}
