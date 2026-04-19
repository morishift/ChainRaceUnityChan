// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace ChainPattern
{
    /// <summary>
    /// Chain that waits for an Animator state to finish playing
    /// </summary>
    public class ChainAnimator : Chain
    {
        Animator animator;
        string stateName;
        int layer;
        CancellationTokenSource cts;

        /// <summary>
        /// Creates an animator chain that plays the specified state
        /// </summary>
        public ChainAnimator(Animator animator, string stateName, int layer = 0)
        {
            this.animator = animator;
            this.stateName = stateName;
            this.layer = layer;
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            if (animator == null)
            {
                Complete();
                return;
            }
            if (isFastForward)
            {
                // Jump to the last frame immediately
                animator.Play(stateName, layer, 1f);
                animator.Update(0f);
                Complete();
                return;
            }
            animator.speed = 1.0f; // Ensure the animator is playing at normal speed
            animator.Play(stateName, layer, 0f);
            animator.Update(0.0f); // Force the state transition to reflect immediately
            cts = new CancellationTokenSource();
            WaitForAnimationAsync(cts.Token).Forget();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            // Debug.Log($"ChainAnimator:SkipInternal stateName:{stateName}");
            cts?.Cancel();
            // Jump to the last frame when skipped
            if (animator != null)
            {
                animator.Play(stateName, layer, 1f);
                animator.Update(0f);
            }
        }

        /// <summary>
        /// Waits until the animation state has finished playing
        /// </summary>
        private async UniTask WaitForAnimationAsync(CancellationToken token)
        {
            // Debug.Log($"ChainAnimator:WaitForAnimationAsync1 stateName:{stateName}");
            try
            {
                // Phase 1: Wait until the state transition is reflected
                while (!animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName))
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                // Phase 2: Wait until the animation finishes
                while (animator.GetCurrentAnimatorStateInfo(layer).normalizedTime < 1.0f)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                // Debug.Log("ChainAnimator:WaitForAnimationAsync2");
                Complete();
            }
            catch (OperationCanceledException) { }
            finally
            {
                // Debug.Log("ChainAnimator:WaitForAnimationAsync3");
                cts?.Dispose();
                cts = null;
            }
        }
    }
}
