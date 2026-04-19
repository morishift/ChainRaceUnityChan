// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace ChainPattern
{
    /// <summary>
    /// Base class for all Chain types.
    /// Manages state transitions (Ready -> Started -> Completed/Skipped)
    /// and provides the UniTask-based async interface.
    /// </summary>
    public abstract class Chain
    {
        enum ChainState
        {
            Ready,
            Started,
            Skipped,
            Completed,
        }

        UniTaskCompletionSource<bool> currentUtcs;
        ChainState chainState;
        Action onComplete;

#if UNITY_EDITOR
        // ---- Debug fields (editor-only) ----

        // Timestamps (Time.realtimeSinceStartup) recorded at start and completion/skip.
        // Used by DebugElapsedSeconds to compute how long the Chain has been running.
        // completedTime stays at -1 while the Chain is still running.
        float startedTime = -1f;
        float completedTime = -1f;
#endif

        public Chain()
        {
            chainState = ChainState.Ready;
        }

        /// <summary>
        /// Starts execution of the Chain.
        /// Returns a UniTask that completes when the Chain finishes or is skipped.
        /// </summary>
        public UniTask Start()
        {
            if (chainState != ChainState.Ready)
            {
                // If already started, return the existing task to wait for it
                return currentUtcs?.Task ?? UniTask.CompletedTask;
            }

            currentUtcs = new UniTaskCompletionSource<bool>();
            chainState = ChainState.Started;
#if UNITY_EDITOR
            startedTime = Time.realtimeSinceStartup;
#endif
            StartInternal();
            return currentUtcs.Task;
        }

        /// <summary>
        /// Skips the Chain, transitioning it immediately to its final state.
        /// The completion callback is NOT invoked on skip.
        /// </summary>
        public void Skip()
        {
            if (chainState != ChainState.Started)
            {
                return;
            }
            chainState = ChainState.Skipped;
#if UNITY_EDITOR
            completedTime = Time.realtimeSinceStartup;
#endif
            onComplete = null; // Don't call completion callback when skipped
            SkipInternal();
            currentUtcs?.TrySetResult(true);
        }

        /// <summary>
        /// Sets the callback to invoke when this Chain completes normally (not skipped).
        /// </summary>
        public void SetCompleteCallback(Action callback)
        {
            onComplete = callback;
        }

        /// <summary>
        /// Sets whether this Chain should fast-forward to its final state immediately on Start.
        /// </summary>
        public void SetIsFastForward(bool fastForward)
        {
            isFastForward = fastForward;
        }

        /// <summary>
        /// Marks the Chain as completed. Must be called by derived classes when their work is done.
        /// Must NOT be called from SkipInternal().
        /// </summary>
        protected void Complete()
        {
            if (chainState != ChainState.Started)
            {
                return;
            }
            chainState = ChainState.Completed;
#if UNITY_EDITOR
            completedTime = Time.realtimeSinceStartup;
#endif
            onComplete?.Invoke();
            onComplete = null;
            currentUtcs?.TrySetResult(true);
        }

        /// <summary>
        /// When true, the Chain should skip animations/delays and jump to its final state immediately.
        /// Checked inside StartInternal() to decide whether to fast-forward.
        /// </summary>
        protected bool isFastForward
        {
            get;
            private set;
        }

        /// <summary>
        /// Implement the Chain's main logic here.
        /// Call Complete() when done. If isFastForward is true, jump to the final state and call Complete() immediately.
        /// </summary>
        protected abstract void StartInternal();

        /// <summary>
        /// Implement immediate transition to the final state here.
        /// Do NOT call Complete() — the Chain infrastructure handles task resolution after Skip().
        /// </summary>
        protected abstract void SkipInternal();

#if UNITY_EDITOR
        // ---- Debug properties (editor-only) ----
        // These are used by ChainDebugWindow to display the Chain tree at runtime.

        /// <summary>The type name of this Chain instance.</summary>
        public string DebugTypeName => GetType().Name;

        /// <summary>Whether this Chain was started with fast-forward enabled.</summary>
        public bool DebugIsFastForward => isFastForward;

        /// <summary>The current state as a string: "Ready", "Started", "Skipped", or "Completed".</summary>
        public string DebugState => chainState.ToString();

        /// <summary>
        /// Seconds elapsed since this Chain started.
        /// Returns -1 if not yet started. Freezes at the completion/skip time once finished.
        /// </summary>
        public float DebugElapsedSeconds
        {
            get
            {
                if (startedTime < 0f) return -1f;
                float end = completedTime >= 0f ? completedTime : Time.realtimeSinceStartup;
                return end - startedTime;
            }
        }

        /// <summary>
        /// Child Chains for tree display. Composite Chains (Sequence/Parallel/Race) override this
        /// to return all registered children regardless of their state.
        /// </summary>
        public virtual Chain[] DebugChildren => System.Array.Empty<Chain>();
#endif
    }
}

