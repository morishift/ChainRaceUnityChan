// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ChainPattern.Editor
{
    /// <summary>
    /// EditorWindow that displays the status of a Chain tree in real time during Play Mode.
    /// Call Watch(chain) from game code to register the Chain to observe.
    /// Open via Window > Chain Debug.
    /// </summary>
    public class ChainDebugWindow : EditorWindow
    {
        static Chain watchedChain;
        Vector2 scrollPos;
        bool showCompleted = true;

        Chain lastWatchedChain;
        readonly Dictionary<Chain, double> completionTimes = new();

        static readonly Color ColorReady     = new Color(0.6f, 0.6f, 0.6f);
        static readonly Color ColorStarted   = new Color(0.4f, 1.0f, 0.4f);
        static readonly Color ColorSkipped   = new Color(1.0f, 0.85f, 0.2f);
        static readonly Color ColorCompleted = new Color(0.5f, 0.8f, 1.0f);

        [MenuItem("Window/Chain Debug")]
        public static void ShowWindow()
        {
            GetWindow<ChainDebugWindow>("Chain Debug");
        }

        /// <summary>
        /// Registers a Chain to observe. Call this from game code during Play Mode.
        /// Typically called each time a new root Chain is started (e.g., at the top of ChainStart()).
        /// </summary>
        public static void Watch(Chain chain)
        {
            watchedChain = chain;
        }

        void OnEnable()
        {
            EditorApplication.update += OnUpdate;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        void OnUpdate()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }

        void OnGUI()
        {
            if (lastWatchedChain != watchedChain)
            {
                completionTimes.Clear();
                lastWatchedChain = watchedChain;
            }

            if (watchedChain == null)
            {
                EditorGUILayout.HelpBox(
                    "No Chain is being watched.\nCall ChainDebugWindow.Watch(chain) from game code.",
                    MessageType.Info);
                return;
            }

            showCompleted = EditorGUILayout.ToggleLeft("Show Completed", showCompleted);

            CountChains(watchedChain, out int ready, out int started, out int skipped, out int completed);
            int total = ready + started + skipped + completed;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Total: {total}", GUILayout.ExpandWidth(false));
            DrawColorLabel($"Ready: {ready}", ColorReady);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            DrawColorLabel($"Started: {started}",     ColorStarted);
            DrawColorLabel($"Skipped: {skipped}",     ColorSkipped);
            DrawColorLabel($"Completed: {completed}", ColorCompleted);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            DrawChain(watchedChain, 0);
            EditorGUILayout.EndScrollView();
        }

        void DrawLegend()
        {
            EditorGUILayout.BeginHorizontal();
            DrawColorLabel("Ready",     ColorReady);
            DrawColorLabel("Started",   ColorStarted);
            DrawColorLabel("Skipped",   ColorSkipped);
            DrawColorLabel("Completed", ColorCompleted);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void DrawColorLabel(string text, Color color)
        {
            var prev = GUI.color;
            GUI.color = color;
            GUILayout.Label($"■ {text}", GUILayout.ExpandWidth(false));
            GUI.color = prev;
        }

        void CountChains(Chain chain, out int ready, out int started, out int skipped, out int completed)
        {
            ready = started = skipped = completed = 0;
            CountChainsRecursive(chain, ref ready, ref started, ref skipped, ref completed);
        }

        void CountChainsRecursive(Chain chain, ref int ready, ref int started, ref int skipped, ref int completed)
        {
            switch (chain.DebugState)
            {
                case "Ready":     ready++;     break;
                case "Started":   started++;   break;
                case "Skipped":   skipped++;   break;
                case "Completed": completed++; break;
            }
            foreach (var child in chain.DebugChildren)
            {
                CountChainsRecursive(child, ref ready, ref started, ref skipped, ref completed);
            }
        }

        GUIStyle richLabelStyle;
        GUIStyle RichLabelStyle => richLabelStyle ??= new GUIStyle(EditorStyles.label) { richText = true };

        static string ColorHex(Color c) => $"#{(int)(c.r*255):X2}{(int)(c.g*255):X2}{(int)(c.b*255):X2}";

        void DrawChain(Chain chain, int depth)
        {
            string state = chain.DebugState;

            if (state == "Completed" && !completionTimes.ContainsKey(chain))
                completionTimes[chain] = EditorApplication.timeSinceStartup;

            if (!showCompleted && state == "Completed")
            {
                if (EditorApplication.timeSinceStartup - completionTimes[chain] >= 1.0)
                    return;
            }

            Color stateColor = state switch
            {
                "Ready"     => ColorReady,
                "Started"   => ColorStarted,
                "Skipped"   => ColorSkipped,
                "Completed" => ColorCompleted,
                _           => Color.white,
            };

            float elapsed = chain.DebugElapsedSeconds;
            string elapsedStr = elapsed >= 0f ? $"  {elapsed:F1}s" : "";
            string ffStr = chain.DebugIsFastForward ? "  <color=#FF88FF>[FF]</color>" : "";
            string hex = ColorHex(stateColor);

            EditorGUI.indentLevel = depth;
            EditorGUILayout.LabelField(
                $"<color={hex}>{chain.DebugTypeName}  [{state}]{elapsedStr}</color>{ffStr}",
                RichLabelStyle);

            foreach (var child in chain.DebugChildren)
            {
                DrawChain(child, depth + 1);
            }
        }
    }
}
