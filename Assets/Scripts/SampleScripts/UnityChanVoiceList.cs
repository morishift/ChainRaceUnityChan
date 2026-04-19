// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    /// <summary>
    /// Loads unity-chan_voice_list.json and provides lookup by ID for dialogue text and AudioClip
    /// </summary>
    public static class UnityChanVoiceList
    {
        const string JsonPath = "UnityChan/Voice/unity-chan_voice_list";
        const string WavFolder = "UnityChan/Voice/";

        [System.Serializable]
        class VoiceEntry
        {
            public string id;
            public string text;
        }

        [System.Serializable]
        class VoiceEntryList
        {
            public VoiceEntry[] items;
        }

        static Dictionary<string, VoiceEntry> table;

        static void EnsureLoaded()
        {
            if (table != null) return;
            table = new Dictionary<string, VoiceEntry>();

            var asset = Resources.Load<TextAsset>(JsonPath);
            if (asset == null)
            {
                Debug.LogError($"[UnityChanVoiceList] JSON not found: Resources/{JsonPath}");
                return;
            }

            var wrapper = JsonUtility.FromJson<VoiceEntryList>(asset.text);
            if (wrapper?.items == null) return;

            foreach (var entry in wrapper.items)
            {
                if (!string.IsNullOrEmpty(entry.id))
                {
                    table[entry.id] = entry;
                }
            }
        }

        /// <summary>
        /// Returns the dialogue text for the given ID (e.g. "univ0002").
        /// Returns null if the ID is not found.
        /// </summary>
        public static string GetText(string id)
        {
            EnsureLoaded();
            return table.TryGetValue(id, out var entry) ? entry.text : null;
        }

        /// <summary>
        /// Loads and returns the AudioClip for the given ID (e.g. "univ0002").
        /// Returns null if the wav file is not found.
        /// </summary>
        public static AudioClip LoadAudioClip(string id)
        {
            return Resources.Load<AudioClip>(WavFolder + id);
        }
    }
}
