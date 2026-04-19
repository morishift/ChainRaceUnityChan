// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Kenichi Morishita

using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ChainPattern
{
    /// <summary>
    /// Chain with onStart, onSkip, onUpdate events
    /// </summary>
    public class ChainWork : Chain
    {
        /// <summary>
        /// Event invoked when execution starts
        /// </summary>
        public event Action onStart;
        /// <summary>
        /// Event invoked when skipped
        /// </summary>
        public event Action onSkip;
        /// <summary>
        /// Event invoked every frame after ChainWork starts
        /// </summary>
        public event Action onUpdate;

        CancellationTokenSource cts;
        bool isStarted;

        public ChainWork()
        {
        }

        /// <summary>
        /// Indicates whether the work will be skipped
        /// </summary>
        public bool isWorkFastForward => isFastForward;

        /// <summary>
        /// Ends the work execution
        /// </summary>
        public void End()
        {
            if (isStarted)
            {
                cts?.Cancel();
                isStarted = false;
                Complete();
            }
        }

        /// <summary>
        /// Starts execution
        /// </summary>
        protected override void StartInternal()
        {
            isStarted = true;
            cts = new CancellationTokenSource();
            onStart?.Invoke();
            FrameLoopAsync(cts.Token).Forget();
        }

        /// <summary>
        /// Called when skipped
        /// </summary>
        protected override void SkipInternal()
        {
            isStarted = false;
            cts?.Cancel();
            onSkip?.Invoke();
        }

        /// <summary>
        /// Execute FrameLoop
        /// </summary>
        private async UniTask FrameLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    onUpdate?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                // Executed when canceled                
            }
            finally
            {
                // Dispose of resources after completion or cancellation
                cts?.Dispose();
                cts = null;
            }
        }
    }
}

