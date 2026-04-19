// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace ChainPattern
{
    /// <summary>
    /// Chain that triggers an Animator CrossFade and completes when the target state is entered.
    /// </summary>
    public class ChainCrossFadeWaitState : Chain
    {
        Animator animator;
        string stateName;
        float crossFadeDuration;
        int layer;
        CancellationTokenSource cts;

        /// <summary>
        /// Creates a ChainCrossFadeWaitState.
        /// </summary>
        /// <param name="animator">Target Animator</param>
        /// <param name="stateName">Destination state name. Completes when this state is entered.</param>
        /// <param name="crossFadeDuration">CrossFade blend duration in seconds</param>
        /// <param name="layer">Animator layer index</param>
        public ChainCrossFadeWaitState(Animator animator, string stateName, float crossFadeDuration, int layer = 0)
        {
            this.animator = animator;
            this.stateName = stateName;
            this.crossFadeDuration = crossFadeDuration;
            this.layer = layer;
        }

        protected override void StartInternal()
        {
            if (animator == null)
            {
                Complete();
                return;
            }
            if (isFastForward)
            {
                animator.Play(stateName, layer, 0f);
                animator.Update(0f);
                Complete();
                return;
            }

            animator.CrossFadeInFixedTime(stateName, crossFadeDuration, layer);
            cts = new CancellationTokenSource();
            WaitAsync(cts.Token).Forget();
        }

        protected override void SkipInternal()
        {
            cts?.Cancel();
            if (animator != null)
            {
                animator.Play(stateName, layer, 0f);
                animator.Update(0f);
            }
        }

        async UniTask WaitAsync(CancellationToken token)
        {
            try
            {
                while (animator.IsInTransition(layer) ||
                       !animator.GetCurrentAnimatorStateInfo(layer).IsName(stateName))
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
                Complete();
            }
            catch (OperationCanceledException) { }
            finally
            {
                cts?.Dispose();
                cts = null;
            }
        }
    }
}
