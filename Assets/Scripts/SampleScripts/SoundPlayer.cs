using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    /// <summary>
    /// Component for playing sound effects
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        static SoundPlayer instance;
        const int MaxAudioSourceCount = 10;

        [SerializeField]
        AudioClip[] seClips;

        List<AudioSource> audioSources = new List<AudioSource>();
        List<int> audioSourceOrder = new List<int>();

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            for (int i = 0; i < MaxAudioSourceCount; ++i)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.dopplerLevel = 0; // No doppler effect for sound effects
                audioSources.Add(source);                
                audioSourceOrder.Add(i);
            }
        }

        /// <summary>
        /// Get the length of the sound effect by type. Returns 0 if the type is invalid.
        /// </summary>
        public float GetSELength(SEType sound)
        {
            int index = (int)sound;
            if (index >= seClips.Length || seClips[index] == null)
            {
                return 0f;
            }
            return seClips[index].length;
        }

        /// <summary>
        /// Plays a sound by type
        /// </summary>
        public int PlaySE(SEType sound)
        {
            int index = (int)sound;
            if (index >= seClips.Length)
            {
                return -1;
            }
            return PlayAudioClip(seClips[(int)sound]);
        }

        /// <summary>
        /// Stops the sound playing on the specified audio source index. Returns the index if successful, or -1 if the index is invalid.
        /// </summary>
        /// <param name="audioSourceIndex"></param>
        /// <returns></returns>
        public int Stop(int audioSourceIndex)
        {
            if (audioSourceIndex < 0 || audioSourceIndex >= audioSources.Count)
            {
                return -1;
            }
            var source = audioSources[audioSourceIndex];
            source.Stop();
            return audioSourceIndex;
        }

        /// <summary>
        /// Plays an AudioClip. If all AudioSources are currently playing, the oldest one will be stopped and reused.
        /// return the index of the AudioSource used to play the clip
        /// </summary>
        public int PlayAudioClip(AudioClip clip)
        {
            // Find a free AudioSource in LRU order; if none, recycle the oldest one.
            int orderIndex = audioSourceOrder.FindIndex(i => !audioSources[i].isPlaying);
            if (orderIndex < 0)
            {
                orderIndex = 0;
            }
            int audioSourceIndex = audioSourceOrder[orderIndex];
            audioSourceOrder.RemoveAt(orderIndex);
            audioSourceOrder.Add(audioSourceIndex);
            var source = audioSources[audioSourceIndex];
            source.clip = clip;
            source.Play();
            return audioSourceIndex;
        }

        /// <summary>
        /// Gets the SoundPlayer instance
        /// </summary>
        public static SoundPlayer Get()
        {
            return instance;
        }
    }
}

